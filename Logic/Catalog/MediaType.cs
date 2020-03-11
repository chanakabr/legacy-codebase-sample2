using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Core.Catalog
{
    [DataContract]
    [Serializable]
    public class MediaType
    {
        [DataMember]
        [JsonProperty("m_sTypeName")]
        public string m_sTypeName;

        [DataMember]
        [JsonProperty("m_nTypeID")]
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