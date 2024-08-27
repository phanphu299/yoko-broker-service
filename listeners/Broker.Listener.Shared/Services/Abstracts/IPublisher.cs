using Broker.Listener.Shared.Models;

namespace Broker.Listener.Shared.Services.Abstracts;

public interface IPublisher
{
    Task SendAsync<T>(T message, Func<Task> onSuccess = null) where T : IngestionMessage;
    Task SendAsync<T>(T message, string topicName, Func<Task> onSuccess = null) where T : IngestionMessage;
    Task<bool> SendAsync<T>(T message) where T : IngestionMessage;
}
