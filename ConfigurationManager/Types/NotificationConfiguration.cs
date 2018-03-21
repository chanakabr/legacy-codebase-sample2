namespace ConfigurationManager
{
    public class NotificationConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue BroadcastTasksNum;
        public NumericConfigurationValue BroadcastThreadSleep;
        public NumericConfigurationValue BroadcastMinMessagesNumForTasks;
        public NumericConfigurationValue BroadcastThreadSleepIndicator;
        

        public NotificationConfiguration(string key) : base(key)
        {
            BroadcastTasksNum = new NumericConfigurationValue("broadcast_tasks_num", this)
            {
                DefaultValue = 10,
                Description = "Original key is NOTIFICATION_BROADCAST_TASKS_NUM"
            };
            BroadcastThreadSleep = new NumericConfigurationValue("broadcast_thread_sleep", this)
            {
                DefaultValue = 50,
                Description = "Original key is NOTIFICATION_BROADCAST_THREAD_SLEEP"
            };
            BroadcastMinMessagesNumForTasks = new NumericConfigurationValue("broadcast_min_messages_num_for_tasks", this)
            {
                DefaultValue = 100,
                Description = "Original key is NOTIFICATION_BROADCAST_MIN_MESSAGES_NUM_FOR_TASKS"
            };
            BroadcastThreadSleepIndicator = new NumericConfigurationValue("broadcast_thread_sleep_indicator", this)
            {
                DefaultValue = 100,
                Description = "Original key is NOTIFICATION_BROADCAST_THREAD_SLEEP_INDICATOR"
            };
        }
    }
}