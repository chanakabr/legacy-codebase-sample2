using System;
using System.Threading.Tasks;
using ApiLogic.Notification;
using KLogMonitor;

namespace NotificationHandlers.Common
{
    public class IotNotificationService : IIotNotificationService
    {
        private const int MAX_DELAY_BETWEEN_NOTIFICATIONS_IN_MS = 30 * 1000; // 30 seconds

        private static readonly IKLogger Logger = new KLogger(nameof(IotNotificationService));

        private readonly IIotManager _iotManager;

        public IotNotificationService(IIotManager iotManager)
        {
            _iotManager = iotManager ?? throw new ArgumentNullException(nameof(iotManager));
        }

        public async Task SendNotificationAsync(int groupId, string message, string topicFormat)
        {
            var partitionsCount = _iotManager.GetTopicPartitionsCount();
            var delay = partitionsCount == 1 ? 0 : MAX_DELAY_BETWEEN_NOTIFICATIONS_IN_MS / (partitionsCount - 1);
            for (var partitionNumber = 0; partitionNumber < partitionsCount; partitionNumber++)
            {
                var topic = string.Format(topicFormat, partitionNumber);
                using (AppMetrics.Iot.RequestDuration())
                {
                    try
                    {
                        if (_iotManager.PublishIotMessage(groupId, message, topic))
                        {
                            AppMetrics.Iot.RequestSucceed();
                            Logger.Debug($"Iot: Message '{message}' sent to topic: {topic}");
                        }
                        else
                        {
                            IotSendFailed(topic, message);
                        }
                    }
                    catch (Exception exception)
                    {
                        IotSendFailed(topic, message, exception);
                        throw;
                    }
                }
                await Task.Delay(delay);
            }
        }

        private static void IotSendFailed(string topic, string message, Exception exception = null)
        {
            Logger.Error($"Iot: Failed to send message `{message}` to topic: {topic}", exception);
            AppMetrics.Iot.RequestFailed();
        }
    }
}