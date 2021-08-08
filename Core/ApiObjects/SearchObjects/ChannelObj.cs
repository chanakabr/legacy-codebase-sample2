using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class ChannelObj
    {
        public int m_nChannelID;
        public int m_nChannelTypeID;
        public int m_nGroupID;
        public int m_nParentGroupID;
        public int m_nIsActive;
        public int m_nStatus;
        public int m_nMediaType;
        public List<string> medias;
        public OrderBy m_eOrderBy;
        public OrderDir m_eOrderDir;
        public List<SearchValue> m_lTags;


        public CutWith m_eCutWith;

        public ChannelObj()
        {
            m_nChannelID = 0;
            m_nChannelTypeID = 0;
            m_nGroupID = 0;
            m_nIsActive = 0;
            m_nStatus = 0;
            m_eCutWith = CutWith.OR;
            m_nMediaType = 0;
            m_nParentGroupID = 0;
            medias = new List<string>();
            m_eOrderBy = OrderBy.ID;
            m_eOrderDir = OrderDir.ASC;

            m_lTags = new List<SearchValue>();
        }
    }
    [Serializable]
    [JsonObject(Id = "orderobj")]
    [DataContract]
    public class OrderObj
    {
        [DataMember]
        [JsonProperty()]
        public OrderBy m_eOrderBy;
        [DataMember]
        [JsonProperty()]
        public OrderDir m_eOrderDir;
        [DataMember]
        [JsonProperty()]
        public string m_sOrderValue
        {
            get;
            set;
        }

        //SlidingWindow
        [DataMember]
        [JsonProperty()]
        public int lu_min_period_id;
        [DataMember]
        [JsonProperty()]
        public bool m_bIsSlidingWindowField
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty()]
        public bool isSlidingWindowFromRestApi { get; set; }

        [JsonProperty()]
        public DateTime m_dSlidingWindowStartTimeField
        {
            get
            {
                if (isSlidingWindowFromRestApi)
                {
                    return DateTime.UtcNow.AddMinutes(-lu_min_period_id);
                }
                else
                {
                    return GetSlidingWindowStart(lu_min_period_id);
                }
            }
        }


        [DataMember]
        [JsonProperty()]
        public bool shouldPadString { get; set; }

        public OrderObj()
        {
            m_eOrderBy = OrderBy.ID;
            m_eOrderDir = OrderDir.DESC;

            m_sOrderValue = string.Empty;
        }

        [DataMember]
        [JsonProperty()]
        public DateTime? trendingAssetWindow { get; set; }

        private static DateTime GetSlidingWindowStart(int minPeriodId)
        {
            return Duration.GetSlidingWindowStart(minPeriodId);
        }
    }

    [DataContract]
    public enum OrderBy
    {
        [EnumMember]
        ID = 0,
        [EnumMember]
        [SlidingWindowSupported]
        VIEWS = -7,
        [EnumMember]
        [SlidingWindowSupported]
        RATING = -8,
        [EnumMember]
        [SlidingWindowSupported]
        VOTES_COUNT = -80,
        [EnumMember]
        [SlidingWindowSupported]
        LIKE_COUNTER = -9,
        [EnumMember]
        START_DATE = -10,
        [EnumMember]
        NAME = -11,
        [EnumMember]
        CREATE_DATE = -12,
        [EnumMember]
        META = 100,
        [EnumMember]
        RANDOM = -6,
        [EnumMember]
        RELATED = 31,
        [EnumMember]
        NONE = 101,
        [EnumMember]
        RECOMMENDATION = -13,
        [EnumMember]
        UPDATE_DATE = -14

        /*
    [EnumMember]
    ID = 0,
    [EnumMember]
    META1_STR = 1,
    [EnumMember]
    META2_STR = 2,
    [EnumMember]
    META3_STR = 3,
    [EnumMember]
    META4_STR = 4,
    [EnumMember]
    META5_STR = 5,
    [EnumMember]
    META6_STR = 6,
    [EnumMember]
    META7_STR = 7,
    [EnumMember]
    META8_STR = 8,
    [EnumMember]
    META9_STR = 9,
    [EnumMember]
    META10_STR = 10,
    [EnumMember]
    META11_STR = 11,
    [EnumMember]
    META12_STR = 12,
    [EnumMember]
    META13_STR = 13,
    [EnumMember]
    META14_STR = 14,
    [EnumMember]
    META15_STR = 15,
    [EnumMember]
    META16_STR = 16,
    [EnumMember]
    META17_STR = 17,
    [EnumMember]
    META18_STR = 18,
    [EnumMember]
    META19_STR = 19,
    [EnumMember]
    META20_STR = 20,
    [EnumMember]
    META1_DOUBLE = 21,
    [EnumMember]
    META2_DOUBLE = 22,
    [EnumMember]
    META3_DOUBLE = 23,
    [EnumMember]
    META4_DOUBLE = 24,
    [EnumMember]
    META5_DOUBLE = 25,
    [EnumMember]
    META6_DOUBLE = 26,
    [EnumMember]
    META7_DOUBLE = 27,
    [EnumMember]
    META8_DOUBLE = 28,
    [EnumMember]
    META9_DOUBLE = 29,
    [EnumMember]
    META10_DOUBLE = 30,
    [EnumMember]
    RELATED = 31,
    [EnumMember]
    META = 100,
    [EnumMember]
    NONE = 101,
    [EnumMember]
    RANDOM = -6,
    [EnumMember]
    VIEWS = -7,
    [EnumMember]
    RATING = -8,
    [EnumMember]
    LIKE_COUNTER = -9,
    [EnumMember]
    START_DATE = -10,
    [EnumMember]
    NAME = -11,
    [EnumMember]
    CREATE_DATE = -12,
    [EnumMember]
    VOTES_COUNT = -80
         * */

    }
    [DataContract]
    public class SlidingWindowSupportedAttribute : Attribute
    {

    }

    [DataContract]
    public enum OrderDir
    {
        [EnumMember]
        ASC = 0,
        [EnumMember]
        DESC = 1,
        [EnumMember]
        NONE = 2,

    }

    [DataContract]
    [Serializable]
    public enum CutWith
    {
        [EnumMember]
        WCF_ONLY_DEFAULT_VALUE = 0,
        [EnumMember]
        OR = 1,
        [EnumMember]
        AND = 2
    }
}
