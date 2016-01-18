using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class UnifiedSearchDefinitions :  BaseSearchObject
    {
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

        public OrderObj order;

        public bool shouldUseFinalEndDate;
        public bool shouldUseStartDate;
        public bool shouldIgnoreDeviceRuleID = false;
        public bool shouldAddActive = true;

        public string permittedWatchRules
        {
            get;
            set;
        }

        /// <summary>
        /// The important part - the tree of filter conditions, connected with Ands/Ors.
        /// </summary>
        public BooleanPhraseNode filterPhrase;

        /// <summary>
        /// Whether or not use the default start date range filter or not
        /// </summary>
        public bool defaultStartDate;
        /// <summary>
        /// Whether or not use the default end date range filter or not
        /// </summary>
        public bool defaultEndDate;

        public int groupId
        {
            get;
            set;
        }
        public int indexGroupId
        {
            get;
            set;
        }

        public LanguageObj langauge
        {
            get;
            set;
        }
        
        public int userTypeID;

        public int[] deviceRuleId;

        /// <summary>
        /// In case search is on medias, list of media types IDs to search for (Episode, movie etc.)
        /// </summary>
        public List<int> mediaTypes;

        /// <summary>
        /// Are EPGs relevant to this search or not
        /// </summary>
        public bool shouldSearchEpg;

        /// <summary>
        /// Are media relevant to this search or not
        /// </summary>
        public bool shouldSearchMedia;

        /// <summary>
        /// Fields that will show in the result in addition to the basic, default return fields
        /// </summary>
        public List<string> extraReturnFields;

        /// <summary>
        /// How many days forward and backward do we search for EPGs
        /// </summary>
        public double epgDaysOffest;

        /// <summary>
        /// Which regions should the linear media belong to
        /// </summary>
        public List<int> regionIds;

        /// <summary>
        /// List of Ids of linear channel media types
        /// </summary>
        public List<string> linearChannelMediaTypes;

        /// <summary>
        /// Mapping of which media types and their parents
        /// </summary>
        public Dictionary<int, int> parentMediaTypes;

        /// <summary>
        /// Mapping of association tag by child media type
        /// </summary>
        public Dictionary<int, string> associationTags;

        /// <summary>
        /// List of Ids of geo block rules that the media belong to
        /// </summary>
        public List<int> geoBlockRules;

        /// <summary>
        /// List of tags and their values that the user needs to enter parental PIN to watch
        /// </summary>
        public Dictionary<string, List<string>> mediaParentalRulesTags;

        /// <summary>
        /// List of tags and their values that the user needs to enter parental PIN to watch
        /// </summary>
        public Dictionary<string, List<string>> epgParentalRulesTags;

        /// <summary>
        /// Lists of specific assets that should be returned in query
        /// </summary>
        public Dictionary<eAssetTypes, List<string>> specificAssets;

        /// <summary>
        /// List of IDs in required order of return
        /// </summary>
        public List<long> specificOrder;

        #endregion

        #region Ctor

        public UnifiedSearchDefinitions()
        {
            pageIndex = 0;
            pageSize = 0;
            groupId = 0;
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
        }

        #endregion
    }
}
