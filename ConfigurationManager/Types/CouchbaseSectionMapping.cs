using ConfigurationManager.ConfigurationSettings.ConfigurationBase;

using System.Collections.Generic;

namespace ConfigurationManager
{
    public class CouchbaseSectionMapping : BaseConfig<CouchbaseSectionMapping>
    {
        public override string TcmKey => TcmObjectKeys.CouchbaseSectionMapping;

        public override string[] TcmPath => new string[] { TcmKey };

        public BaseValue<string> Tokens = new BaseValue<string>("tokens", "OTT_Apps");
        public BaseValue<string> Groups = new BaseValue<string>("groups", "OTT_Apps");
        public BaseValue<string> EPG = new BaseValue<string>("epg", "epg_channels_schedule");
        public BaseValue<string> MediaMarks = new BaseValue<string>("mediamark", "MediaMarks");
        public BaseValue<string> DomainConcurrency = new BaseValue<string>("domain_concurrency", "domain_concurrency");
        public BaseValue<string> MediaHits = new BaseValue<string>("media_hit", "media_hit");
        public BaseValue<string> Statistics = new BaseValue<string>("statistics", "statistics");
        public BaseValue<string> Recordings = new BaseValue<string>("recordings", "OTT_Apps");
        public BaseValue<string> Cache = new BaseValue<string>("cache","Cache");
        public BaseValue<string> Social = new BaseValue<string>("social", "social");
        public BaseValue<string> ScheduledTasks = new BaseValue<string>("scheduled_tasks", "scheduled_tasks");
        public BaseValue<string> Memcached = new BaseValue<string>("memcached", "GroupsCache");
        public BaseValue<string> Notifications = new BaseValue<string>("notification", "OTT_Apps");
        public BaseValue<string> OTTApps = new BaseValue<string>("ott_apps", "OTT_Apps");

 

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