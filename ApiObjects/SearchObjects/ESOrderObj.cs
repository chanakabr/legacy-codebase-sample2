using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace ApiObjects.SearchObjects
{
    [DataContract]
    public class ESOrderObj
    {
        [DataMember]
        public OrderDir m_eOrderDir;

        [DataMember]
        public string m_sOrderValue;

        public ESOrderObj()
        {
            m_eOrderDir = OrderDir.ASC;
            m_sOrderValue = string.Empty;
        }         


    }
}
