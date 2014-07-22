using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Script.Serialization;

namespace ApiObjects.MediaIndexingObjects
{
    [Serializable]
    public class IndexingData : QueueObject
    {
        #region Properties

        [DataMember]
        public List<int> Ids { get; set; }
        [DataMember]
        public eObjectType ObjectType { get; set; }
        [DataMember]
        public eAction Action { get; set; }
        [DataMember]
        public long Date { get; set; }

        #endregion

        #region CTOR

        public IndexingData()
        { }

        public IndexingData(List<int> lIds, int nGroupId, eObjectType eUpdateObj, eAction eAction)
        {
            this.GroupId = nGroupId;
            this.Ids = lIds;
            this.ObjectType = eUpdateObj;
            this.Action = eAction;
        }

        #endregion

        public IndexingData(List<int> lIds, int nGroupId, eObjectType eUpdateObj, eAction eAction, long date)
            : this(lIds, nGroupId, eUpdateObj, eAction)
        {
            this.Date = date;
        }
    }
}
