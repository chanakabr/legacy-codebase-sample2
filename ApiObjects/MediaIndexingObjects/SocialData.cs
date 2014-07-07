using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.MediaIndexingObjects
{
    public class SocialData : QueueObject
    {
        #region Members

        [DataMember]
        public string ActorSiteGuid { get; set; }
        [DataMember]
        public string DbActionId { get; set; }

        #endregion

        #region CTOR

        public SocialData()
        {           
 
        }

        public SocialData(int nGroupId, string sSiteGuid, string sDbActionId)
        {
            this.GroupId = nGroupId;
            this.ActorSiteGuid = sSiteGuid;
            this.DbActionId = sDbActionId;
        }

        #endregion
    }
}
