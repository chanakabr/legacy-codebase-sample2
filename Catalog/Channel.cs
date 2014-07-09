using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Catalog
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "channel")]
    public class Channel
    {
        #region Members
        [DataMember]
        public int m_nChannelID { get; set; }
        [DataMember]
        public int m_nChannelTypeID { get; set; }
        [DataMember]
        public int m_nGroupID { get; set; }
        [DataMember]
        public int m_nParentGroupID { get; set; }
        [DataMember]
        public int m_nIsActive { get; set; }
        [DataMember]
        public int m_nStatus { get; set; }
        [DataMember]
        public int m_nMediaType { get; set; }
        [DataMember]
        public string m_sName { get; set; }
        [DataMember]
        public string m_sDescription { get; set; }
        [DataMember]
        public List<string> m_sMedias { get; set; }
        [DataMember]
        public List<int> m_oMedias { get; set; }
        [DataMember]
        public ApiObjects.SearchObjects.OrderBy m_eOrderBy { get; set; }
        [DataMember]
        public ApiObjects.SearchObjects.OrderDir m_eOrderDir { get; set; }
        [DataMember]
        public ApiObjects.SearchObjects.CutWith m_eCutWith { get; set; }
        [DataMember]
        public List<ApiObjects.SearchObjects.SearchValue> m_lChannelTags { get; set; }
        [DataMember]
        public List<ManualMedia> m_lManualMedias { get; set; } // Populated when the channel is manual
        [DataMember]
        public ApiObjects.SearchObjects.OrderObj m_OrderObject { get; set; }
        
        #endregion

        #region CTOR

        public Channel()
        {
            m_nChannelID = 0;
            m_nChannelTypeID = 0;
            m_nGroupID = 0;
            m_nIsActive = 0;
            m_nStatus = 0;
            m_eCutWith  = ApiObjects.SearchObjects.CutWith.OR;
            m_nMediaType = 0;
            m_nParentGroupID = 0;
            m_oMedias = new List<int>();
            m_sMedias = new List<string>();
            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID;
            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
        }

        #endregion
    }
}
