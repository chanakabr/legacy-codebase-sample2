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

        public bool m_bSearchOnlyDatesAndChannels;
        public bool m_bIsCurrent;
        public int m_nNextTop;
        public int m_nPrevTop;

        public bool m_bSearchAnd;
        public bool m_bDesc;
        public string m_sOrderBy;

        public bool m_bExact;

        public List<SearchValue> m_lSearchAnd
        {
            get;
            set;
        }

        public int m_nPageIndex
        {
            get;
            set;
        }
        public int m_nPageSize
        {
            get;
            set;
        }

        public OrderObj m_oOrder;
        public CutWith m_eCutWith;

        public bool m_bUseFinalEndDate;
        public bool m_bUseStartDate;

        public string m_sName;
        public string m_sDescription;
        public string m_sMediaTypes;
        public string m_sPermittedWatchRules
        {
            get;
            set;
        }

        public List<SearchValue> m_dAnd;
        public List<SearchValue> m_dOr;
        public List<SearchValue> m_lFilterTagsAndMetas
        {
            get;
            set;
        }
        public CutWith m_eFilterTagsAndMetasCutWith
        {
            get;
            set;
        }

        public int m_nGroupId
        {
            get;
            set;
        }
        public int m_nIndexGroupId
        {
            get;
            set;
        }

        public LanguageObj m_oLangauge
        {
            get;
            set;
        }
        public int m_nUserTypeID;

        public int[] m_nDeviceRuleId;
        public int[] m_nMediaFileTypes;

        public UnifiedQueryType m_QueryType;

        public List<string> m_ExtraReturnFields;

        #endregion

        #region Ctor

        public UnifiedSearchDefinitions()
        {
            m_nPageIndex = 0;
            m_nPageSize = 0;
            m_nGroupId = 0;
            m_bSearchAnd = false;
            m_bDesc = false;
            m_bExact = false;
            m_lSearchAnd = new List<SearchValue>();


            m_lSearchAnd = new List<SearchValue>();

            m_bSearchOnlyDatesAndChannels = false;
            m_nNextTop = 0;
            m_nPrevTop = 0;
            m_bIsCurrent = false;

            m_sMediaTypes = string.Empty;

            m_eCutWith = CutWith.OR;
            m_bExact = false;
            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_bUseFinalEndDate = false;

            m_dAnd = new List<SearchValue>();
            m_dOr = new List<SearchValue>();

            m_bUseStartDate = true;

            m_nDeviceRuleId = null;
            m_nMediaFileTypes = null;

            m_oOrder = new OrderObj();

            // By default search on both EPG and MEDIA
            m_QueryType = UnifiedQueryType.All;

            m_ExtraReturnFields = new List<string>();
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
