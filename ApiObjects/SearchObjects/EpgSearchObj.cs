using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class EpgSearchObj
    {
        public bool m_bSearchAnd;
        public bool m_bDesc;
        public string m_sOrderBy;
     
        public List<EpgSearchValue> m_lSearch { get; set; }  

        public DateTime m_dStartDate { get; set; }
        public DateTime m_dEndDate { get; set; }
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

            m_bSearchAnd = false;
            m_bDesc = false;

            m_eInterCutWith = CutWith.OR;

            m_lSearch = new List<EpgSearchValue>();          
        }

        public bool ContainSearchKey(string key, ref EpgSearchValue searchVal)
        {
            foreach (EpgSearchValue item in this.m_lSearch)
            {
                if (item.m_sValue.ToLower() == key.ToLower())
                {
                    searchVal =  item;
                    return true;
                }
            }
            searchVal = new EpgSearchValue();
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

