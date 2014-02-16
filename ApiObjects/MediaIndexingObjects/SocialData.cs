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
        public int ActorSiteGuid { get; set; }
        [DataMember]
        public string DbActionId { get; set; }

        #endregion

        #region CTOR

        public SocialData()
        {           
 
        }

        public SocialData(int nGroupId, int nSiteGuid, string sDbActionId)
        {
            this.GroupId = nGroupId;
            this.ActorSiteGuid = nSiteGuid;
            this.DbActionId = sDbActionId;
        }

        #endregion
    }
}
