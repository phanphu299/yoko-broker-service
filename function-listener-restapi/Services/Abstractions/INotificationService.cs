using System.Threading.Tasks;
using AHI.Broker.Function.Model;

namespace AHI.Broker.Function.Service.Abstraction
{
    public interface INotificationService
    {
        Task SendNotifyAsync(string endpoint, NotificationMessage message);
    }
}