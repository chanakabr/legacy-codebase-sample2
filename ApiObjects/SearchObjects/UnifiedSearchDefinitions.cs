using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class UnifiedSearchDefinitions
    {
        #region Data Members

        public bool shouldSearchAnd;
        public bool isDescending;
        public string orderBy;

        public bool isExact;

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

        public string name;
        public string description;
        public string mediaTypes;
        public string permittedWatchRules
        {
            get;
            set;
        }

        public List<SearchValue> andList;
        public List<SearchValue> orList;
        public List<SearchValue> filterTagsAndMetas
        {
            get;
            set;
        }
        public CutWith filterTagsAndMetasCutWith
        {
            get;
            set;
        }

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
        public int[] mediaFileTypes;

        public UnifiedQueryType queryType;

        public List<string> extraReturnFields;

        #endregion

        #region Ctor

        public UnifiedSearchDefinitions()
        {
            pageIndex = 0;
            pageSize = 0;
            groupId = 0;
            shouldSearchAnd = false;
            isDescending = false;
            isExact = false;

            mediaTypes = string.Empty;

            isExact = false;
            name = string.Empty;
            description = string.Empty;

            shouldUseFinalEndDate = false;

            andList = new List<SearchValue>();
            orList = new List<SearchValue>();

            shouldUseStartDate = true;

            deviceRuleId = null;
            mediaFileTypes = null;

            order = new OrderObj();

            // By default search on both EPG and MEDIA
            queryType = UnifiedQueryType.All;

            extraReturnFields = new List<string>();
        }

        #endregion
    }

    public enum UnifiedQueryType
    {
        All,
        Media,
        EPG
    }
}
