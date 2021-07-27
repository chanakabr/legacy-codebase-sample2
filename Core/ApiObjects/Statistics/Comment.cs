using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects.Statistics
{
    public class Comment : SocialActionStatistics
    {
        [DataMember]
        [JsonIgnore]
        public Int32 Id;
        [DataMember]
        [JsonProperty("writer")]
        public string m_sWriter;
        [DataMember]
        [JsonProperty("language_id")]
        public int m_nLang;
        [DataMember]
        [JsonIgnore]
        public string m_sLangName;
        [DataMember]
        [JsonProperty("header")]
        public string m_sHeader;
        [DataMember]
        [JsonProperty("sub_header")]
        public string m_sSubHeader;
        [DataMember]
        [JsonProperty("text")]
        public string m_sContentText;
        [DataMember]
        [JsonIgnore]
        public string m_sUserPicURL;

        public Comment()
        {
            this.Action = "comment"; //defult value - comment
        }

    }
}
