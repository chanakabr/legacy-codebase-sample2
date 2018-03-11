using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class CouchBaseDesigns : ConfigurationValue
    {
        public StringConfigurationValue MediaMarkDesign;
        public StringConfigurationValue EPGDesign;
        public StringConfigurationValue QueueMessagesDesign;
        public StringConfigurationValue SocialFeedDesign;
        public StringConfigurationValue SearchHistoryDesign;

        public CouchBaseDesigns(string key) : base(key)
        {
            MediaMarkDesign = new StringConfigurationValue("media_mark", this)
            {
                DefaultValue = "mediamark"
            };
            EPGDesign = new StringConfigurationValue("epg", this)
            {
                DefaultValue = "epg"
            };
            QueueMessagesDesign = new StringConfigurationValue("queue_messages", this)
            {
                DefaultValue = "queue_messages"
            };
            SocialFeedDesign = new StringConfigurationValue("social_feed", this)
            {
                DefaultValue = "socialfeed"
            };
            SearchHistoryDesign = new StringConfigurationValue("search_history", this)
            {
                DefaultValue = "searchHistory"
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