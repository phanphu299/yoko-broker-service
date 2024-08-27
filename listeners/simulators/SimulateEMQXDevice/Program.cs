using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimulateEMQXDevice;


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<EmqxPublisherMqtt>();
        services.AddScoped<EmqxPublisherCoap>();
        services.AddTransient<IDictionary<string, IEmqxPublisher>>(sp =>
            new Dictionary<string, IEmqxPublisher>
            {
                { "MQTT", sp.GetRequiredService<EmqxPublisherMqtt>() },
                { "COAP", sp.GetRequiredService<EmqxPublisherCoap>() },
            });
        services.AddTransient<Program>();
    })
    .Build();

var program = host.Services.GetRequiredService<Program>();
await program.StartAsync();

public partial class Program
{
    private readonly IConfiguration _configuration;
    private readonly IDictionary<string, IEmqxPublisher> _emqxPublisher;
    public Program(IConfiguration configuration
        , IDictionary<string, IEmqxPublisher> emqxPublisher
    )
    {
        _configuration = configuration;
        _emqxPublisher = emqxPublisher;
    }

    public async Task StartAsync()
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
            Console.WriteLine("Sample execution cancellation requested; will exit.");
        };
        Console.WriteLine($"{DateTime.Now}> Press Control+C at any time to quit the sample.");

        _ = bool.TryParse(_configuration["UsingMqtt"], out bool usingMqtt);
        var protocol = usingMqtt ? "MQTT" : "COAP";
        var publisher = _emqxPublisher[protocol];
        try
        {
            await publisher.RunAsync(cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            await publisher.StopAsync();
        }
    }
}
