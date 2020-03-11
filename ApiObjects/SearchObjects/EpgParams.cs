using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects.SearchObjects
{
    /*this search objects finds all the EPG programs with the relevant channel ID and group ID and also:
     *in case the EpgSearchType is "ByDate": the query will retreive all the programs that started after the "m_dStartDate" and ended before the "m_dEndDate"
     *in case the EpgSearchType is "Current": the query will retreive all the programs that:
     *started before "now" and ended after "now"
     *OR
     *the "m_nNextTop" next programs that started after "now"
     *OR
     *the "m_nPrevTop" previous programs that ended before "now"  
     */
    [DataContract]
        public class EpgParams
        {
            public List<int> m_lchannelIDs { get; set; }

            //indicates if the search is for specific dates or for the current programs and the next "m_nNextTop" and the previous "m_nPrevTop" programs
            public EpgSearchType m_eSearchType { get; set; } 

            public int m_nNextTop { get; set; }

            public int m_nPrevTop { get; set; }

            public EpgParams()
            {                        
                m_lchannelIDs = new List<int>();
                m_eSearchType = EpgSearchType.ByDate;
                m_nNextTop = 0;
                m_nPrevTop = 0;
            }
    }
}
