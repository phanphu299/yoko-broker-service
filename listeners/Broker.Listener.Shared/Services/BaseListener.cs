using System.Collections.Concurrent;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Internal;
using Broker.Listener.Shared.Exceptions;
using Broker.Listener.Shared.Models;
using Broker.Listener.Shared.Services.Abstracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Broker.Listener.Shared.Services;

public abstract class BaseListener : BackgroundService
{
    protected readonly string _topic;
    protected readonly bool _useMultiTopic;
    private bool _resourceMonitorSet = false;
    private readonly SemaphoreSlim _concurrencyCollectorLock;
    private readonly Queue<int> _queueCounts;
    private readonly Queue<int> _availableCounts;
    private readonly ILogger<BaseListener> _logger;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentBag<MqttClientWrapper> _mqttClients;
    private readonly IResourceMonitor _resourceMonitor;
    private readonly IFuzzyThreadController _fuzzyThreadController;
    private readonly IDynamicRateLimiter _dynamicRateLimiter;
    protected readonly IPublisher _publisher;
    private System.Timers.Timer _concurrencyCollector;
    private CancellationToken _stoppingToken;

    public BaseListener(
        ILogger<BaseListener> logger
        , IConfiguration configuration
        , IResourceMonitor resourceMonitor
        , IFuzzyThreadController fuzzyThreadController
        , IDynamicRateLimiter dynamicRateLimiter
        , IPublisher publisher)
    {
        _logger = logger;
        _configuration = configuration;
        _resourceMonitor = resourceMonitor;
        _fuzzyThreadController = fuzzyThreadController;
        _publisher = publisher;
        _dynamicRateLimiter = dynamicRateLimiter;
        _dynamicRateLimiter.SetLimit(_configuration.GetValue<int>("Concurrency:InitialConcurrencyLimit")).Wait();
        _mqttClients = new ConcurrentBag<MqttClientWrapper>();
        _concurrencyCollectorLock = new SemaphoreSlim(1);
        _queueCounts = new Queue<int>();
        _availableCounts = new Queue<int>();
        ConfiguredProjects = GetConfiguredProjects();
        _topic = _configuration["Kafka:DefaultTopic"] ?? "ingestion";
        _useMultiTopic = _configuration.GetValue<bool>("Kafka:UseMultiTopic");
    }

    protected IEnumerable<ProjectInfo> ConfiguredProjects { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;
        SetupCancellationTokens();
        StartConcurrencyCollector();
        StartDynamicScalingWorker();

        var factory = new MqttFactory();
        var mqttClientConfiguration = _configuration.GetSection("Mqtt");

        var noOfConns = mqttClientConfiguration.GetValue<int>("NumberOfConnections");
        for (int i = 0; i < noOfConns; i++)
            await InitializeMqttClient(factory, mqttClientConfiguration, i);

        while (!stoppingToken.IsCancellationRequested)
            await Task.Delay(1000, stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        var shutdownWait = _configuration.GetValue<int>("Concurrency:ShutdownWait");
        var topic = _configuration["Mqtt:Topic"];
        // [TODO] graceful shutdown
        foreach (var wrapper in _mqttClients)
        {
            if (wrapper.Client.IsConnected)
            {
                await wrapper.Client.UnsubscribeAsync(topic);
                await wrapper.Client.InternalClient.DisconnectAsync(new MqttClientDisconnectOptions
                {
                    SessionExpiryInterval = 1,
                    Reason = MqttClientDisconnectOptionsReason.AdministrativeAction
                });
                await wrapper.Client.StopAsync();
            }
        }
        await Task.Delay(shutdownWait);
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        base.Dispose();
        foreach (var wrapper in _mqttClients)
            wrapper.Client.Dispose();
        _mqttClients.Clear();
        _concurrencyCollectorLock?.Dispose();
        _queueCounts?.Clear();
        _availableCounts?.Clear();
        _resourceMonitor?.Stop();
        _concurrencyCollector?.Dispose();
    }

    public abstract Task HandleMessage(MqttApplicationMessageReceivedEventArgs e, MqttClientWrapper wrapper);

    private IEnumerable<ProjectInfo> GetConfiguredProjects()
    {
        var projectInfo = _configuration["ProjectInfo"] ?? string.Empty;
        // _logger.LogDebug("PROJECT_INFO --- {projectInfo}", projectInfo);

        var projectInfoStr = projectInfo.Split(';');
        IEnumerable<ProjectInfo> projectInfos = projectInfoStr.Where(x => !string.IsNullOrEmpty(x) && x.Split('_').Length >= 3).Select(x =>
        {
            var infoArr = x.Split('_');
            return ProjectInfo.Create(infoArr[0], infoArr[1], infoArr[2]);
        });

        return projectInfos;
    }

    protected async Task SendIngestionMessage(IngestionMessage message, Func<Task> onSuccess)
    {
        try
        {
            message.TopicName = _useMultiTopic ? $"{_topic}-{Guid.Parse(message.ProjectId):N}" : _topic;
            await _publisher.SendAsync(message, onSuccess);
        }
        catch (Exception ex)
        {
            throw new DownstreamDisconnectedException(ex.Message, ex);
        }
    }



    private void SetupCancellationTokens()
    {
        _stoppingToken.Register(() =>
        {
            foreach (var wrapper in _mqttClients)
                wrapper.TokenSource.TryCancel();
        });
    }

    private void StartDynamicScalingWorker()
    {
        if (!_resourceMonitorSet)
        {
            _resourceMonitorSet = true;
            var concurrencyConfiguration = _configuration.GetSection("Concurrency");
            var scaleFactor = concurrencyConfiguration.GetValue<int>("ScaleFactor");
            var initialConcurrencyLimit = concurrencyConfiguration.GetValue<int>("InitialConcurrencyLimit");
            var acceptedAvailableConcurrency = concurrencyConfiguration.GetValue<int>("AcceptedAvailableConcurrency");
            var acceptedQueueCount = concurrencyConfiguration.GetValue<int>("AcceptedQueueCount");
            _resourceMonitor.SetMonitor(async (cpu, mem) =>
            {
                try
                {
                    await ScaleConcurrency(cpu, mem, scaleFactor, initialConcurrencyLimit, acceptedAvailableConcurrency, acceptedQueueCount);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            }, interval: concurrencyConfiguration.GetValue<int>("ScaleCheckInterval"));
        }
        _resourceMonitor.Start();
    }

    private async Task ScaleConcurrency(double cpu, double mem, int scaleFactor, int initialConcurrencyLimit, int acceptedAvailableConcurrency, int acceptedQueueCount)
    {
        var threadScale = _fuzzyThreadController.GetThreadScale(cpu, mem, factor: scaleFactor);
        if (threadScale == 0)
            return;
        var (queueCountAvg, availableCountAvg) = await GetConcurrencyStatistics();
        var (concurrencyLimit, _, _, _) = _dynamicRateLimiter.State;
        int newLimit;
        if (threadScale < 0)
            newLimit = concurrencyLimit + threadScale;
        else if (queueCountAvg <= acceptedQueueCount && availableCountAvg > acceptedAvailableConcurrency)
            newLimit = concurrencyLimit - threadScale / 2;
        else
            newLimit = concurrencyLimit + threadScale;
        if (newLimit < initialConcurrencyLimit)
            newLimit = initialConcurrencyLimit;
        await _dynamicRateLimiter.SetLimit(newLimit, cancellationToken: _stoppingToken);
        _logger.LogDebug(
            "CPU: {cpu} - Memory: {mem}\n" +
            "Scale: {threadScale} - Available count: {availableCountAvg} - Queue count: {queueCountAvg}\n" +
            "New thread limit: {newLimit}",
            cpu, mem, threadScale, availableCountAvg, queueCountAvg, newLimit);
    }

    private void StartConcurrencyCollector()
    {
        if (_concurrencyCollector == null)
        {
            var movingAvgRange = _configuration.GetValue<int>("Concurrency:MovingAverageRange");
            var collectorInterval = _configuration.GetValue<int>("Concurrency:ConcurrencyCollectorInterval");
            _concurrencyCollector = new System.Timers.Timer(collectorInterval)
            {
                AutoReset = true
            };
            _concurrencyCollector.Elapsed += async (s, e) =>
            {
                await _concurrencyCollectorLock.WaitAsync(_stoppingToken);
                try
                {
                    if (_queueCounts.Count == movingAvgRange)
                        _queueCounts.TryDequeue(out var _);
                    if (_availableCounts.Count == movingAvgRange)
                        _availableCounts.TryDequeue(out var _);
                    var (_, _, concurrencyAvailable, concurrencyQueueCount) = _dynamicRateLimiter.State;
                    _queueCounts.Enqueue(concurrencyQueueCount);
                    _availableCounts.Enqueue(concurrencyAvailable);
                }
                finally { _concurrencyCollectorLock.Release(); }
            };
        }
        _concurrencyCollector.Start();
    }

    private async Task<(int QueueCountAvg, int AvailableCountAvg)> GetConcurrencyStatistics()
    {
        int queueCountAvg;
        int availableCountAvg;
        await _concurrencyCollectorLock.WaitAsync(_stoppingToken);
        try
        {
            queueCountAvg = _queueCounts.Count > 0 ? (int)_queueCounts.Average() : 0;
            availableCountAvg = _availableCounts.Count > 0 ? (int)_availableCounts.Average() : 0;
            return (queueCountAvg, availableCountAvg);
        }
        finally { _concurrencyCollectorLock.Release(); }
    }

    private async Task InitializeMqttClient(MqttFactory factory, IConfigurationSection mqttClientConfiguration, int threadIdx)
    {
        var mqttClient = factory.CreateManagedMqttClient();
        var optionsBuilder = new MqttClientOptionsBuilder()
            .WithTcpServer(mqttClientConfiguration["TcpServer"])
            .WithCleanSession(value: mqttClientConfiguration.GetValue<bool>("CleanSession"))
            .WithSessionExpiryInterval(mqttClientConfiguration.GetValue<uint>("SessionExpiryInterval"))
            .WithProtocolVersion(MqttProtocolVersion.V500)
            .WithCredentials(username: mqttClientConfiguration["UserName"], password: mqttClientConfiguration["Password"]);

        var clientId = mqttClientConfiguration["ClientId"] ?? "";
        var podName = _configuration["PodName"] ?? Guid.NewGuid().ToString();
        optionsBuilder = optionsBuilder.WithClientId($"{clientId}_{podName}_{threadIdx}");

        var options = optionsBuilder.Build();
        var managedOptions = new ManagedMqttClientOptionsBuilder()
            .WithAutoReconnectDelay(TimeSpan.FromSeconds(mqttClientConfiguration.GetValue<int>("ReconnectDelaySecs")))
            .WithClientOptions(options)
            .Build();
        var wrapper = new MqttClientWrapper(mqttClient, _stoppingToken);
        _mqttClients.Add(wrapper);

        mqttClient.ConnectedAsync += (e) => OnConnected(e, mqttClient);
        mqttClient.DisconnectedAsync += (e) => OnDisconnected(e, wrapper);
        mqttClient.ApplicationMessageReceivedAsync += (e) => OnMessageReceived(e, wrapper);

        await mqttClient.StartAsync(managedOptions);
    }

    private async Task OnConnected(MqttClientConnectedEventArgs e, IManagedMqttClient mqttClient)
    {
        _logger.LogDebug("### CONNECTED WITH SERVER - ClientId: {ClientId} ###", mqttClient.Options.ClientOptions.ClientId);
        var topic = _configuration["Mqtt:Topic"];
        var qos = _configuration.GetValue<MQTTnet.Protocol.MqttQualityOfServiceLevel>("Mqtt:QoS");
        try
        {
            await mqttClient.SubscribeAsync(topic: topic, qualityOfServiceLevel: qos);
            _logger.LogDebug("### SUBSCRIBED topic {topic} - qos {qos} ###", topic, qos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "### FAILED TO SUBSCRIBE MQTT TOPIC ### reason: {Message}", ex.Message);
            throw;
        }
    }

    private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e, MqttClientWrapper wrapper)
    {
        try
        {
            e.AutoAcknowledge = false;
            await _dynamicRateLimiter.Acquire(cancellationToken: wrapper.TokenSource.Token);
            var _ = Task.Run(async () =>
            {
                try
                { await HandleMessage(e, wrapper); }
                catch (DownstreamDisconnectedException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    // [TODO] handle open/close circuit
                }
                catch (MessageSkippedException ex)
                {
                    _logger.LogError(ex.Message);
                    await e.AcknowledgeAsync(wrapper.TokenSource.Token);
                }
                catch (Exception ex) { _logger.LogError(ex, ex.Message); }
                finally { await _dynamicRateLimiter.Release(); }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    private Task OnDisconnected(MqttClientDisconnectedEventArgs e, MqttClientWrapper wrapper)
    {
        wrapper.TokenSource.TryCancel();
        _logger.LogError(e.Exception, "### DISCONNECTED FROM SERVER ### {Event}", e.Exception == null ? JsonSerializer.Serialize(e) : e.Exception.Message);
        return Task.CompletedTask;
    }
}

public class MqttClientWrapper
{
    private CancellationTokenSource _tokenSource;
    private readonly IManagedMqttClient _client;
    private readonly CancellationToken _stoppingToken;

    public MqttClientWrapper(IManagedMqttClient client, CancellationToken stoppingToken)
    {
        _client = client;
        _stoppingToken = stoppingToken;
        ResetTokenSource();
    }

    public IManagedMqttClient Client => _client;
    public CancellationTokenSource TokenSource => _tokenSource;

    public void ResetTokenSource()
    {
        _tokenSource?.Dispose();
        if (_stoppingToken.IsCancellationRequested)
            return;
        _tokenSource = new CancellationTokenSource();
        _tokenSource.Token.Register(() => ResetTokenSource());
    }
}
