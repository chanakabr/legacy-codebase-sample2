using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ServiceModel;
using Newtonsoft.Json;
namespace ApiObjects.SearchObjects
{
    [DataContract]
    [Serializable]
    [JsonObject(Id = "searchvalue")]
    public class SearchValue
    {
        public string m_sKey;
        public List<string> m_lValue;
        public string m_sValue;
        public CutWith m_eInnerCutWith { get; set; }
        public string m_sKeyPrefix { get; set; }

        public SearchValue()
        {
            m_sKey = string.Empty;
            m_sKeyPrefix = string.Empty;
            m_lValue = new List<string>();
            m_sValue = string.Empty;
            m_eInnerCutWith = CutWith.OR;
        }

        public SearchValue(string key, string val)
        {
            m_sKey = key;
            m_sValue = val;
            m_sKeyPrefix = string.Empty;
            m_eInnerCutWith = CutWith.OR;
        }

        public SearchValue(string key, string val, string sKeyPrefix)
        {
            m_sKey = key;
            m_sValue = val;
            m_sKeyPrefix = string.Empty;
            m_eInnerCutWith = CutWith.OR;
            m_sKeyPrefix = sKeyPrefix;
        }
    }


    [Serializable]
    [JsonObject(ItemTypeNameHandling=TypeNameHandling.Auto)]
    public class MediaSearchObj : BaseSearchObject
    {
        [JsonProperty()]
        [DataMember]
        public OrderObj m_oOrder;
        [JsonProperty()]
        [DataMember]
        public CutWith m_eCutWith;

        [JsonProperty()]
        [DataMember]
        public bool m_bExact;
        [JsonProperty()]
        [DataMember]
        public bool m_bUseFinalEndDate;
        [JsonProperty()]
        [DataMember]
        public bool m_bUseStartDate;
        [JsonProperty()]
        [DataMember]
        public bool m_bUseActive;
        [JsonProperty()]
        [DataMember]
        public bool m_bIgnoreDeviceRuleId;

        [JsonProperty()]
        [DataMember]
        public string m_sName;
        [JsonProperty()]
        [DataMember]
        public string m_sDescription;
        [JsonProperty()]
        [DataMember]
        public string m_sMediaTypes;
        [JsonProperty()]
        [DataMember]
        public string m_sPermittedWatchRules
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public List<SearchValue> m_dAnd;
        [JsonProperty()]
        [DataMember]
        public List<SearchValue> m_dOr;
        [JsonProperty()]
        [DataMember]
        public List<SearchValue> m_lFilterTagsAndMetas
        {
            get;
            set;
        }
        [JsonProperty()]
        [DataMember]
        public CutWith m_eFilterTagsAndMetasCutWith
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public int m_nGroupId
        {
            get;
            set;
        }
        [JsonProperty()]
        [DataMember]
        public int m_nIndexGroupId
        {
            get;
            set;
        }
        [JsonProperty()]
        [DataMember]
        public int m_nPageIndex
        {
            get;
            set;
        }
        [JsonProperty()]
        [DataMember]
        public int m_nPageSize
        {
            get;
            set;
        }
        [JsonProperty()]
        [DataMember]
        public int m_nMediaID;
        [JsonProperty()]
        [DataMember]
        public LanguageObj m_oLangauge
        {
            get;
            set;
        }
        [JsonProperty()]
        [DataMember]
        public int m_nUserTypeID;

        [JsonProperty()]
        [DataMember]
        public int[] m_nDeviceRuleId;
        [JsonProperty()]
        [DataMember]
        public int[] m_nMediaFileTypes;

        /*
         * 1. The following two lists are used for IPNO filtering in Eutelsat.
         * 2. In IPNO filtering the search created by the above properties must also satisfy the IPNO's channels definitions or
         * 3. not satisfy all channels of all ipnos. 
         * 4. This logic is because user associated with IPNO in Eutelsat can either access medias associated with his IPNO
         * 5. or medias which are not associated with any IPNO (including his IPNO).
         * 6. Hence, we pass to m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt the definitions of his IPNO channels
         * 7. (The definitions are extracted from ES Percolator). m_lOrMediaNotInAnyOfTheseChannelsDefinitions receives the
         * 8. definitions of all the channels of all ipnos.
         */
        [JsonProperty()]
        [DataMember]
        public List<string> m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt;
        [JsonProperty()]
        [DataMember]
        public List<string> m_lOrMediaNotInAnyOfTheseChannelsDefinitions;


        [JsonProperty()]
        [DataMember]
        public List<int> regionIds;

        [JsonProperty()]
        [DataMember]
        public List<string> linearChannelMediaTypes;

        /// <summary>
        /// Mapping of which media types and their parents
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<int, int> parentMediaTypes;

        /// <summary>
        /// Mapping of association tag by child media type
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<int, string> associationTags;

        public MediaSearchObj()
        {
            m_sMediaTypes = string.Empty;

            m_eCutWith = CutWith.OR;
            m_bExact = false;
            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_bUseFinalEndDate = false;

            m_dAnd = new List<SearchValue>();
            m_dOr = new List<SearchValue>();

            m_nMediaID = 0;
            m_bUseStartDate = true;

            m_nDeviceRuleId = null;
            m_nMediaFileTypes = null;

            m_oOrder = new OrderObj();

            m_nPageSize = 0;
            m_nPageIndex = 0;

            m_lChannelsDefinitionsMediaNeedsToBeInAtLeastOneOfIt = new List<string>();
            m_lOrMediaNotInAnyOfTheseChannelsDefinitions = new List<string>();

            regionIds = new List<int>();

            parentMediaTypes = new Dictionary<int, int>();
            associationTags = new Dictionary<int, string>();
        }
    }

    public class SearchRelated
    {
        public int m_nMediaID;
        public float m_nScore;

        public SearchRelated()
        {
            m_nMediaID = 0;
            m_nScore = 0;
        }

        public SearchRelated(int nMediaID, float nScore)
        {
            m_nMediaID = nMediaID;
            m_nScore = nScore;
        }
    }


    public class StatisticsActionSearchObj
    {       
        public CutWith m_eCutWith;

        public int MediaID;
        public int GroupID;
        public string MediaType;
        public DateTime Date;
        public string Action;
        public int RateValue;

        public StatisticsActionSearchObj()
        { 
            m_eCutWith = CutWith.AND;
            GroupID = 0;
            MediaType = string.Empty;
            Date = DateTime.UtcNow;
            Action = string.Empty;
            RateValue = 0;
        }
    }
}
