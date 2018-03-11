using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ConfigurationManager
{
    public class CouchbaseSectionMapping : ConfigurationValue
    {
        public StringConfigurationValue Tokens;
        public StringConfigurationValue Groups;
        public StringConfigurationValue EPG;
        public StringConfigurationValue MediaMarks;
        public StringConfigurationValue DomainConcurrency;
        public StringConfigurationValue MediaHits;
        public StringConfigurationValue Statistics;
        public StringConfigurationValue Recordings;
        public StringConfigurationValue Cache;
        public StringConfigurationValue Social;
        public StringConfigurationValue ScheduledTasks;
        public StringConfigurationValue Memcached;
        public StringConfigurationValue Notifications;
        public StringConfigurationValue OTTApps;

        public CouchbaseSectionMapping(string key) : base(key)
        {
            Tokens = new StringConfigurationValue("tokens", this)
            {
                DefaultValue = "OTT_Apps"
            };
            Groups = new StringConfigurationValue("groups", this)
            {
                DefaultValue = "OTT_Apps"
            };
            MediaMarks = new StringConfigurationValue("mediamark", this)
            {
                DefaultValue = "epg_channels_schedule"
            };
            DomainConcurrency = new StringConfigurationValue("domain_concurrency", this)
            {
                DefaultValue = "domain_concurrency"
            };
            MediaHits = new StringConfigurationValue("media_hits", this)
            {
                DefaultValue = "media_hit"
            };
            Statistics = new StringConfigurationValue("statistics", this)
            {
                DefaultValue = "statistics"
            };
            Recordings = new StringConfigurationValue("recordings", this)
            {
                DefaultValue = "OTT_Apps"
            };
            Cache = new StringConfigurationValue("cache", this)
            {
                DefaultValue = "Cache"
            };
            Social = new StringConfigurationValue("social", this)
            {
                DefaultValue = "social"
            };
            ScheduledTasks = new StringConfigurationValue("scheduled_tasks", this)
            {
                DefaultValue = "scheduled_tasks"
            };
            Memcached = new StringConfigurationValue("memcached", this)
            {
                DefaultValue = "GroupsCache"
            };
            Notifications = new StringConfigurationValue("notification", this)
            {
                DefaultValue = "OTT_Apps"
            };
            OTTApps = new StringConfigurationValue("ott_apps", this)
            {
                DefaultValue = "OTT_Apps"
            };
        }

        internal override bool Validate()
        {
            bool result = true;
            result &= Tokens.Validate();
            result &= Groups.Validate();
            result &= MediaMarks.Validate();
            result &= DomainConcurrency.Validate();
            result &= Statistics.Validate();
            result &= Recordings.Validate();
            result &= Cache.Validate();
            result &= Social.Validate();
            result &= ScheduledTasks.Validate();
            result &= Memcached.Validate();
            result &= Notifications.Validate();
            result &= OTTApps.Validate();

            return result;
        }

        public Dictionary<string, string> GetDictionary()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            result.Add("tokens", this.Tokens.Value);
            result.Add("groups", this.Groups.Value);
            result.Add("epg", this.EPG.Value);
            result.Add("mediamark", this.MediaMarks.Value);
            result.Add("domain_concurrency", this.DomainConcurrency.Value);
            result.Add("media_hits", this.MediaHits.Value);
            result.Add("statistics", this.Statistics.Value);
            result.Add("recordings", this.Recordings.Value);
            result.Add("cache", this.Cache.Value);
            result.Add("social", this.Social.Value);
            result.Add("scheduled_tasks", this.ScheduledTasks.Value);
            result.Add("memcached", this.Memcached.Value);
            result.Add("notification", this.Notifications.Value);
            result.Add("ott_apps", this.OTTApps.Value);
            
            return result;
        }
    }
}