namespace ConfigurationManager
{
    public class CouchBaseDesigns : ConfigurationValue
    {
        public StringConfigurationValue MediaMarkDesign;
        public StringConfigurationValue EPGDesign;
        public StringConfigurationValue QueueMessagesDesign;
        public StringConfigurationValue SocialFeedDesign;
        public StringConfigurationValue SearchHistoryDesign;
        public StringConfigurationValue StatisticsDesign;

        public CouchBaseDesigns(string key) : base(key)
        {
            MediaMarkDesign = new StringConfigurationValue("media_mark", this)
            {
                DefaultValue = "mediamark",
                OriginalKey = "cb_media_mark_design"
            };
            EPGDesign = new StringConfigurationValue("epg", this)
            {
                DefaultValue = "epg",
                OriginalKey = "cb_epg_design"
            };
            QueueMessagesDesign = new StringConfigurationValue("queue_messages", this)
            {
                DefaultValue = "queue_messages",
                OriginalKey = "cb_queue_messages_design"
            };
            SocialFeedDesign = new StringConfigurationValue("social_feed", this)
            {
                DefaultValue = "socialfeed",
                OriginalKey = "cb_feed_design"
            };
            SearchHistoryDesign = new StringConfigurationValue("search_history", this)
            {
                DefaultValue = "searchHistory",
                OriginalKey = "search_history_design_doc"
            };
            StatisticsDesign = new StringConfigurationValue("statistics", this)
            {
                DefaultValue = "statistics",
                OriginalKey = "cb_statistics_design"
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= MediaMarkDesign.Validate();
            result &= EPGDesign.Validate();
            result &= QueueMessagesDesign.Validate();
            result &= SocialFeedDesign.Validate();
            result &= SearchHistoryDesign.Validate();

            return result;
        }
    }
}