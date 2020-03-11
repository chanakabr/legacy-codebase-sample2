using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class PushMessagesConfiguration : BaseConfig<PushMessagesConfiguration>
    {

        public BaseValue<int> NumberOfMessagesPerSecond = new BaseValue<int>("num_of_messages_per_second", 3, false, "");
        public BaseValue<int> TTLSeconds = new BaseValue<int>("ttl_seconds", 30, false, "");

        public override string TcmKey => "push_messages";

        public override string[] TcmPath => new[] { TcmKey };
    }
}
