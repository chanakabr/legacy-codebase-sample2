using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class EpgResultsObj
    {
        [DataMember]
        public int m_nTotalItems;

        [DataMember]
        public int m_nChannelID;       

        [DataMember]
        public List<EPGChannelProgrammeObject> m_lEpgProgram;

        public EpgResultsObj()
        {
            m_lEpgProgram = new List<EPGChannelProgrammeObject>();
        }       
    }
}
