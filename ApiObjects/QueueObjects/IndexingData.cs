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
        public List<long> Ids
        {
            get;
            set;
        }
        [DataMember]
        public eObjectType ObjectType { get; set; }
        [DataMember]
        public eAction Action { get; set; }
        [DataMember]
        public long Date { get; set; }


        [DataMember]
        public List<string> sIds { get; set; }
       

        #endregion

        #region CTOR

        public IndexingData()
        { }

        public IndexingData(List<long> lIds, int nGroupId, eObjectType eUpdateObj, eAction eAction)
        {
            this.GroupId = nGroupId;
            this.Ids = lIds;
            this.ObjectType = eUpdateObj;
            this.Action = eAction;
        }
        public IndexingData(List<string> Ids, int nGroupId, eObjectType eUpdateObj, eAction eAction)
        {
            this.GroupId = nGroupId;
            this.sIds = Ids;
            this.ObjectType = eUpdateObj;
            this.Action = eAction;
        }

        #endregion

        public IndexingData(List<long> lIds, int nGroupId, eObjectType eUpdateObj, eAction eAction, long date)
            : this(lIds, nGroupId, eUpdateObj, eAction)
        {
            this.Date = date;
        }

        public IndexingData(List<string> Ids, int nGroupId, eObjectType eUpdateObj, eAction eAction, long date)
            : this(Ids, nGroupId, eUpdateObj, eAction)
        {
            this.Date = date;
        }
    }
}
