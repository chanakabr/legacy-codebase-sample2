namespace ConfigurationManager
{
    public class NotificationConfiguration : ConfigurationValue
    {
        public NumericConfigurationValue BroadcastTasksNum;
        
        public NotificationConfiguration(string key) : base(key)
        {
            BroadcastTasksNum = new NumericConfigurationValue("broadcast_tasks_num", this)
            {
                DefaultValue = 10,
                Description = "Original key is NOTIFICATION_BROADCAST_TASKS_NUM"
            };           
        }
    }
}