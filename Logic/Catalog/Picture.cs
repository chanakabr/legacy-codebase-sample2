using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog
{
    [DataContract]
    public class Picture
    {
        [DataMember]
        public string m_sSize = string.Empty;

        [DataMember]
        public string m_sURL = string.Empty;

        [DataMember]
        public string ratio = string.Empty;

        [DataMember]
        public int version;

        [DataMember]
        public string id = string.Empty;

        [DataMember]
        public bool isDefault = false;

        public Picture() { }

        public Picture(string sSize, string sURL, string picRatio)
        {
            m_sSize = sSize;
            m_sURL = sURL;
            ratio = picRatio;
        }
    }
}
