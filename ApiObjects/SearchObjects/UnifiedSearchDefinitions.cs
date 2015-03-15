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

        public string permittedWatchRules
        {
            get;
            set;
        }

        //public List<SearchValue> andList;
        //public List<SearchValue> orList;
        //public List<SearchValue> filterTagsAndMetas
        //{
        //    get;
        //    set;
        //}
        //public CutWith filterTagsAndMetasCutWith
        //{
        //    get;
        //    set;
        //}

        public BooleanPhraseNode filterPhrase;

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

        public List<int> mediaTypes;
        public bool shouldSearchEpg;

        public List<string> extraReturnFields;

        #endregion

        #region Ctor

        public UnifiedSearchDefinitions()
        {
            pageIndex = 0;
            pageSize = 0;
            groupId = 0;
            isDescending = false;
            isExact = false;

            mediaTypes = new List<int>();
            shouldSearchEpg = false;

            isExact = false;

            shouldUseFinalEndDate = false;

            //andList = new List<SearchValue>();
            //orList = new List<SearchValue>();

            filterPhrase = null;

            shouldUseStartDate = true;

            deviceRuleId = null;

            order = new OrderObj();

            extraReturnFields = new List<string>();
        }

        #endregion
    }
}
