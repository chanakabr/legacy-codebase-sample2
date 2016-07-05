using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class EpgSearchObj :  BaseSearchObject
    {
        public bool m_bSearchOnlyDatesAndChannels;
        public bool m_bSearchEndDate;
        public bool m_bIsCurrent;
        public int m_nNextTop;
        public int m_nPrevTop;

        public bool m_bSearchAnd;
        public bool m_bDesc;
        public string m_sOrderBy;

        public List<EpgSearchValue> m_lSearch { get; set; }
        public bool m_bExact;
        public List<SearchValue> m_lSearchOr { get; set; }
        public List<SearchValue> m_lSearchAnd { get; set; }

        public DateTime m_dStartDate { get; set; }
        public DateTime m_dEndDate { get; set; }
        public DateTime m_dSearchEndDate { get; set; }
        public int m_nProgramID { get; set; }
        public int m_nGroupID { get; set; }    // ParentGroup  
        public int m_nPageIndex { get; set; }
        public int m_nPageSize { get; set; }
        public CutWith m_eInterCutWith { get; set; }
        public List<long> m_oEpgChannelIDs { get; set; }


        public EpgSearchObj()
        {
            m_nProgramID = 0;
            m_nGroupID = 0;
            m_nPageIndex = 0;
            m_nPageSize = 0;

            m_dStartDate = DateTime.UtcNow;
            m_dEndDate = DateTime.UtcNow.AddDays(7);
            m_dSearchEndDate = DateTime.UtcNow;

            m_bSearchAnd = false;
            m_bDesc = false;
            m_bExact = false;

            m_eInterCutWith = CutWith.OR;

            m_lSearch = new List<EpgSearchValue>();

            m_lSearchOr = new List<SearchValue>();
            m_lSearchAnd = new List<SearchValue>();

            m_bSearchOnlyDatesAndChannels = false;
            m_nNextTop = 0;
            m_nPrevTop = 0;
            m_bIsCurrent = false;

            m_bSearchEndDate = false;
        }

        public bool ContainSearchKey(string key, ref SearchValue searchVal)
        {
            foreach (SearchValue item in this.m_lSearchOr)
            {
                if (item.m_sValue.ToLower() == key.ToLower())
                {
                    searchVal = item;
                    return true;
                }
            }
            foreach (SearchValue item in this.m_lSearchAnd)
            {
                if (item.m_sValue.ToLower() == key.ToLower())
                {
                    searchVal = item;
                    return true;
                }
            }

            searchVal = new SearchValue();
            return false;
        }
    }

    public class EpgSearchValue
    {
        public string m_sKey;
        public string m_sValue;

        public EpgSearchValue()
        {
            m_sKey = string.Empty;
            m_sValue = string.Empty;
        }
        public EpgSearchValue(string sKey, string sVal)
        {
            m_sKey = sKey;
            m_sValue = sVal;
        }
    }
}

