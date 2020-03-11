using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class SocialFeedQueueConfiguration : BaseConfig<SocialFeedQueueConfiguration>
    {
        public BaseValue<string> Task = new BaseValue<string>("task", "distributed_tasks.process_update_social_feed");
        public BaseValue<string> RoutingKey = new BaseValue<string>("routing_key", "PROCESS_UPDATE_SOCIAL_FEED");
        public BaseValue<string> TaskSocialMerge = new BaseValue<string>("taskSocialMerge",  TcmObjectKeys.Stub, true);
        public BaseValue<string> RoutingKeyMerge = new BaseValue<string>("routingKeySocialFeedMerge", TcmObjectKeys.Stub,true);

        public override string TcmKey => TcmObjectKeys.SocialFeedQueueConfiguration;

        public override string[] TcmPath => new string[] { TcmKey };
    }
}