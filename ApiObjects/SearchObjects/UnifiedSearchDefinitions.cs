using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
    [Serializable]
    public class UnifiedSearchDefinitions : BaseSearchObject
    {
        #region Consts

        public const int EPG_ASSET_TYPE = 0;
        public const int RECORDING_ASSET_TYPE = 1;

        #endregion

        #region Data Members

        public int pageIndex
        {
            get;
            set;
        }
        public int pageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates an offset for the Elasticsearch, from which document index to start the response. This is an alternative for page index+size combo
        /// </summary>
        public int from
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public OrderObj order;

        [JsonProperty()]
        [DataMember]
        public bool shouldUseFinalEndDate;
        [JsonProperty()]
        [DataMember]
        public bool shouldUseStartDate;
        [JsonProperty()]
        [DataMember]
        public bool shouldIgnoreDeviceRuleID = false;
        [JsonProperty()]
        [DataMember]
        public bool shouldAddActive = true;

        [JsonProperty()]
        [DataMember]
        public string permittedWatchRules
        {
            get;
            set;
        }

        /// <summary>
        /// The important part - the tree of filter conditions, connected with Ands/Ors.
        /// </summary>
        [JsonIgnore]
        public BooleanPhraseNode filterPhrase;

        private string filterPhraseString;

        [JsonProperty(PropertyName = "filter_phrase")]
        public string filter_phrase
        {
            get
            {
                if (filterPhrase != null)
                {
                    var jObject = Newtonsoft.Json.Linq.JObject.FromObject(filterPhrase);

                    filterPhraseString = BooleanPhraseNode.Serialize(filterPhrase);
                }

                return filterPhraseString;
            }
            set
            {
                filterPhrase = BooleanPhraseNode.Deserialize(value);
                filterPhraseString = value;
            }
        }
        /// <summary>
        /// Whether or not use the default start date range filter or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool defaultStartDate;

        /// <summary>
        /// Whether or not use the default end date range filter or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool defaultEndDate;

        [JsonProperty()]
        [DataMember]
        public int groupId
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public int indexGroupId
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public LanguageObj langauge
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public int userTypeID;

        [JsonProperty()]
        [DataMember]
        public int[] deviceRuleId;

        /// <summary>
        /// In case search is on medias, list of media types IDs to search for (Episode, movie etc.)
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<int> mediaTypes;

        /// <summary>
        /// Are EPGs relevant to this search or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldSearchEpg;

        /// <summary>
        /// Are media relevant to this search or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldSearchMedia;

        /// <summary>
        /// Are recordings  relevant to this search or not
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldSearchRecordings;

        /// <summary>
        /// Fields that will show in the result in addition to the basic, default return fields
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<string> extraReturnFields;

        /// <summary>
        /// How many days forward and backward do we search for EPGs
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public double epgDaysOffest;

        /// <summary>
        /// Which regions should the linear media belong to
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<int> regionIds;

        /// <summary>
        /// List of Ids of linear channel media types
        /// </summary>
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

        /// <summary>
        /// List of Ids of geo block rules that the media belong to
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<int> geoBlockRules;

        /// <summary>
        /// List of tags and their values that the user needs to enter parental PIN to watch
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<string, List<string>> mediaParentalRulesTags;

        /// <summary>
        /// List of tags and their values that the user needs to enter parental PIN to watch
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<string, List<string>> epgParentalRulesTags;

        /// <summary>
        /// Lists of specific assets that should be returned in query
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<eAssetTypes, List<string>> specificAssets;

        /// <summary>
        /// Lists of specific assets that should NOT be returned in query - they will be filtered out
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public Dictionary<eAssetTypes, List<string>> excludedAssets;

        /// <summary>
        /// List of IDs in required order of return
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<long> specificOrder;

        /// <summary>
        /// All definitions regarding entitled assets of the user
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public EntitlementSearchDefinitions entitlementSearchDefinitions;


        /// <summary>
        /// Is Time Shiffted TV Settings For Group
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldUseSearchEndDate;

        /// <summary>
        /// Lists of specific assets that should be returned in query
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public List<string> excludedCrids;

        /// <summary>
        /// Lists of specific assets that should be returned in query
        /// </summary>
        [JsonProperty()]
        [DataMember]

        public bool shouldReturnExtendedSearchResult;
        /// Defines if start/end date KSQL search will be used only for EPG/recordings or for media as well
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public bool shouldDateSearchesApplyToAllTypes;

        [JsonProperty()]
        [DataMember]
        public int exactGroupId
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public Dictionary<string, string> recordingsToDomainRecordingsMapping
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        [JsonConstructor]
        public UnifiedSearchDefinitions()
        {
            pageIndex = 0;
            pageSize = 0;
            from = 0;
            groupId = 0;
            exactGroupId = 0;
            epgDaysOffest = 0;

            shouldSearchEpg = false;
            shouldUseFinalEndDate = false;
            shouldUseStartDate = true;
            defaultEndDate = true;
            defaultStartDate = true;

            filterPhrase = null;
            deviceRuleId = null;

            order = new OrderObj();

            mediaTypes = new List<int>();
            extraReturnFields = new List<string>();
            parentMediaTypes = new Dictionary<int, int>();
            associationTags = new Dictionary<int, string>();
            geoBlockRules = new List<int>();

            mediaParentalRulesTags = new Dictionary<string, List<string>>();
            epgParentalRulesTags = new Dictionary<string, List<string>>();

            entitlementSearchDefinitions = null;

            shouldUseSearchEndDate = false;
            shouldDateSearchesApplyToAllTypes = false;
        }


        #endregion
    }
}
