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

        [JsonProperty()]
        public DateTime m_dSlidingWindowStartTimeField
        {
            get { return GetSlidingWindowStart(lu_min_period_id); }
        }


        public OrderObj()
        {
            m_eOrderBy = OrderBy.ID;
            m_eOrderDir = OrderDir.DESC;

            m_sOrderValue = string.Empty;
        }

        private static DateTime GetSlidingWindowStart(int minPeriodId)
        {
            switch (minPeriodId)
            {
                case 1:
                    return DateTime.UtcNow.AddMinutes(-1);
                case 5:
                    return DateTime.UtcNow.AddMinutes(-5);
                case 10:
                    return DateTime.UtcNow.AddMinutes(-10);
                case 30:
                    return DateTime.UtcNow.AddMinutes(-30);
                case 60:
                    return DateTime.UtcNow.AddHours(-1);
                case 120:
                    return DateTime.UtcNow.AddHours(-2);
                case 180:
                    return DateTime.UtcNow.AddHours(-3);
                case 360:
                    return DateTime.UtcNow.AddHours(-6);
                case 540:
                    return DateTime.UtcNow.AddHours(-9);
                case 720:
                    return DateTime.UtcNow.AddHours(-12);
                case 1080:
                    return DateTime.UtcNow.AddHours(-18);
                case 1440:
                    return DateTime.UtcNow.AddDays(-1);
                case 2880:
                    return DateTime.UtcNow.AddDays(-2);
                case 4320:
                    return DateTime.UtcNow.AddDays(-3);
                case 7200:
                    return DateTime.UtcNow.AddDays(-5);
                case 10080:
                    return DateTime.UtcNow.AddDays(-7);
                case 20160:
                    return DateTime.UtcNow.AddDays(-14);
                case 30240:
                    return DateTime.UtcNow.AddDays(-21);
                case 40320:
                    return DateTime.UtcNow.AddDays(-28);
                case 40321:
                    return DateTime.UtcNow.AddDays(-28);
                case 43200:
                    return DateTime.UtcNow.AddDays(-30);
                case 44600:
                    return DateTime.UtcNow.AddDays(-31);
                case 1111111:
                    return DateTime.UtcNow.AddMonths(-1);
                case 2222222:
                    return DateTime.UtcNow.AddMonths(-2);
                case 3333333:
                    return DateTime.UtcNow.AddMonths(-3);
                case 4444444:
                    return DateTime.UtcNow.AddMonths(-4);
                case 5555555:
                    return DateTime.UtcNow.AddMonths(-5);
                case 6666666:
                    return DateTime.UtcNow.AddMonths(-6);
                case 9999999:
                    return DateTime.UtcNow.AddMonths(-7);
                case 11111111:
                    return DateTime.UtcNow.AddYears(-1);
                case 22222222:
                    return DateTime.UtcNow.AddYears(-2);
                case 33333333:
                    return DateTime.UtcNow.AddYears(-3);
                case 44444444:
                    return DateTime.UtcNow.AddYears(-4);
                case 55555555:
                    return DateTime.UtcNow.AddYears(-5);
                case 66666666:
                    return DateTime.UtcNow.AddYears(-6);
                case 77777777:
                    return DateTime.UtcNow.AddYears(-7);
                case 88888888:
                    return DateTime.UtcNow.AddYears(-8);
                case 99999999:
                    return DateTime.UtcNow.AddYears(-9);
                case 100000000:
                    return DateTime.UtcNow.AddYears(-10);

                default:
                    return DateTime.MinValue;
            }
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
        RECOMMENDATION = -13

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
