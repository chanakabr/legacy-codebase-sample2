using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Catalog
{
    [DataContract]
    public class Picture
    {
        [DataMember]
        public string m_sSize;
        [DataMember]
        public string m_sURL;

        public Picture()
        { }
        public Picture(string sSize, string sURL)
        {
            m_sSize = sSize;
            m_sURL = sURL;
        }
    }
}
