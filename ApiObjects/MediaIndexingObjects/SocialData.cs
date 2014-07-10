using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.MediaIndexingObjects
{
    [Serializable]
    public class SocialFeedRequest : QueueObject
    {
        #region Members

        [DataMember]
        public string ActorSiteGuid { get; set; }
        [DataMember]
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
    public class SocialUnmergeRequest : QueueObject
    {
        #region Members
        [DataMember]
        public string sSiteGuid { get; set; }
        #endregion

        #region CTOR

        public SocialUnmergeRequest()
        {           
 
        }

        public SocialUnmergeRequest(int nGroupId, string sSiteGuid)
        {
            this.GroupId = nGroupId;
            this.sSiteGuid = sSiteGuid;
        }

        #endregion
    }
}
