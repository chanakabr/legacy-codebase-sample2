using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using ApiObjects.SearchObjects;

namespace GroupsCacheManager
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "channel")]
    public class Channel
    {
        #region Consts

        /// <summary>
        /// KSQL channels can hold media types (as defined in media_types table) and EPG.
        /// EPG is represented by this negative number because:
        /// 1. "0" is usually refered to as everything or nothing. It is mostly invalid in DB.
        /// 2. -1 sounds too... generic
        /// 3. 26 is memorable, unique.
        /// </summary>
        public const int EPG_ASSET_TYPE = -26;

        #endregion

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
        public List<int> m_nMediaType { get; set; }
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

        /// <summary>
        /// KSQL filter query - for KSQL channels
        /// </summary>
        [DataMember]
        public string filterQuery
        {
            get;
            set;
        }

        /// <summary>
        /// Based on the KSQL filter query, and assuming it is valid, this is the tree object that represents the filter
        /// </summary>
        [DataMember]
        public ApiObjects.SearchObjects.BooleanPhraseNode filterTree
        {
            get;
            set;
        }

        /// <summary>
        /// Aggregation groupby option - for KSQL channels
        /// </summary>
        [DataMember]
        public SearchAggregationGroupBy searchGroupBy;

        [DataMember]
        public DateTime? CreateDate { get; set; }
        [DataMember]
        public DateTime? UpdateDate { get; set; }

        #endregion

        #region CTOR

        public Channel()
        {
            m_nChannelID = 0;
            m_nChannelTypeID = 0;
            m_nGroupID = 0;
            m_nIsActive = 0;
            m_nStatus = 0;
            m_eCutWith = ApiObjects.SearchObjects.CutWith.OR;
            m_nMediaType = new List<int>();
            m_nParentGroupID = 0;
            m_oMedias = new List<int>();
            m_sMedias = new List<string>();
            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID;
            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
            filterQuery = string.Empty;
            filterTree = null;
            searchGroupBy = null;
        }

        #endregion
    }

    public enum ChannelType
    {
        None = 0,
        Automatic = 1,
        Manual = 2,
        Watcher = 3,
        KSQL = 4
    }
}
