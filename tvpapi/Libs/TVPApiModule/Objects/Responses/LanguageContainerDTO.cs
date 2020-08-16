using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class LanguageContainerDTO
    {
        [DataMember]
        [JsonProperty()]
        public string m_sLanguageCode3 { get; set; }

        [DataMember]
        [JsonProperty()]
        public string m_sValue { get; set; }

        [DataMember]
        [JsonProperty()]
        public bool IsDefault { get; set; }


        public LanguageContainerDTO()
        {

        }

        public LanguageContainerDTO(LanguageContainer source)
        {
            if (source != null)
            {
                this.m_sLanguageCode3 = source.m_sLanguageCode3;
                this.m_sValue = source.m_sValue;
                this.IsDefault = source.IsDefault;
            }
        }
    }
}