using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

namespace ConfigurationManager
{
    public class CouchBaseDesigns : BaseConfig<CouchBaseDesigns>
    {

        public override string TcmKey => TcmObjectKeys.CouchBaseDesigns;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> MediaMarkDesign = new BaseValue<string>("media_mark", "mediamark");
        public BaseValue<string> EPGDesign = new BaseValue<string>("epg", "epg");
        public BaseValue<string> QueueMessagesDesign = new BaseValue<string>("queue_messages", "queue_messages");
        public BaseValue<string> SocialFeedDesign = new BaseValue<string>("social_feed", "socialfeed");
        public BaseValue<string> SearchHistoryDesign = new BaseValue<string>("search_history", "searchHistory");
        public BaseValue<string> StatisticsDesign = new BaseValue<string>("statistics", "statistics");



    }
}