using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class NotificationConfiguration : BaseConfig<NotificationConfiguration>
    {
        public BaseValue<int> BroadcastTasksNum = new BaseValue<int>("broadcast_tasks_num", 10, false, "");
        public BaseValue<int> BroadcastThreadSleep = new BaseValue<int>("broadcast_thread_sleep", 50, false, "");
        public BaseValue<int> BroadcastMinMessagesNumForTasks = new BaseValue<int>("broadcast_min_messages_num_for_tasks", 100, false, "");
        public BaseValue<int> BroadcastThreadSleepIndicator = new BaseValue<int>("broadcast_thread_sleep_indicator", 100, false, "");

        public override string TcmKey => "notification_configuration";

        public override string[] TcmPath => new[] { TcmKey };
    }
}