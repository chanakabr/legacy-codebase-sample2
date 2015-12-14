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

        [DataMember]
        public string ratio;

        [DataMember]
        public int version;

        [DataMember]
        public string id;

        public Picture() { }

        public Picture(string sSize, string sURL, string picRatio)
        {
            m_sSize = sSize;
            m_sURL = sURL;
            ratio = picRatio;
        }
    }
}
