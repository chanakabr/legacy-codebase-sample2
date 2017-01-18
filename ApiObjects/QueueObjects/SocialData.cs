using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.MediaIndexingObjects
{
    [Serializable]
    public class SocialFeedRequest : QueueObject
    {
        #region Members
        public string id;
        public string task;
        public List<object> args;

        [DataMember]
        [JsonProperty("actor_site_guid")]
        public string ActorSiteGuid { get; set; }
        [DataMember]
        [JsonProperty("db_action_id")]
        public string DbActionId { get; set; }

        #endregion

        #region CTOR

        public SocialFeedRequest()
        {           
 
        }

        public SocialFeedRequest(int nGroupId, string sSiteGuid, string sDbActionId)
        {
            this.GroupId = nGroupId;
            this.ActorSiteGuid = sSiteGuid;
            this.DbActionId = sDbActionId;
        }

        #endregion
    }

    [Serializable]
    public class SocialMergeRequest : QueueObject
    {
        #region Members
        [DataMember]
        [JsonProperty("site_guid")]
        public string sSiteGuid { get; set; }
        [JsonProperty("action")]
        public string Action { get; set; }
        #endregion

        #region CTOR

        public SocialMergeRequest()
        {
 
        }

        public SocialMergeRequest(int nGroupId, string sSiteGuid, string action)
        {
            this.GroupId = nGroupId;
            this.sSiteGuid = sSiteGuid;
        }

        #endregion
    }
}
