using System.Threading.Tasks;

namespace NotificationHandlers.Common
{
    public interface IIotNotificationService
    {
        Task SendNotificationAsync(int groupId, string message, string topicFormat);
    }
}