using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog
{
    [DataContract]
    public class MediaType
    {
        [DataMember]
        public string m_sTypeName;
        [DataMember]
        public int m_nTypeID;

        public MediaType()
        { }
        public MediaType(string sTypeName, int nTypeID)
        {
            m_sTypeName = sTypeName;
            m_nTypeID = nTypeID;
        }
    }
}
