using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Google.Protobuf;

namespace ApiObjects
{
    [Serializable]
    [DataContract]
    public class LanguageContainer : IEquatable<LanguageContainer>, IDeepCloneable<LanguageContainer>
    {
        [DataMember]
        [JsonProperty("LanguageCode")]
        public string m_sLanguageCode3 { get; set; }

        [DataMember]
        [JsonProperty("Value")]
        public string m_sValue { get; set; }

        [DataMember]
        [JsonProperty("IsDefault")]
        public bool IsDefault { get; set; }

        public LanguageContainer()
        {
            m_sLanguageCode3 = string.Empty;
            m_sValue = string.Empty;
            IsDefault = false;
        }

        public LanguageContainer(string languageCode, string value)
        {
            m_sLanguageCode3 = languageCode;
            m_sValue = value;
            IsDefault = false;
        }

        public LanguageContainer(string languageCode, string value, bool isDefault)
        {
            m_sLanguageCode3 = languageCode;
            m_sValue = value;
            IsDefault = isDefault;
        }
        
        public LanguageContainer(LanguageContainer other) {
            m_sLanguageCode3 = other.m_sLanguageCode3;
            m_sValue = other.m_sValue;
            IsDefault = other.IsDefault;
        }

        public void Initialize(string languageCode, string value)
        {
            m_sLanguageCode3 = languageCode;
            m_sValue = value;
            IsDefault = false;
        }

        public bool Equals(LanguageContainer other)
        {
            if (other == null)
                return false;

            return m_sLanguageCode3.Equals(other.m_sLanguageCode3) &&
                m_sValue.Equals(other.m_sValue) &&
                IsDefault == other.IsDefault;
        }

        public LanguageContainer Clone()
        {
            return new LanguageContainer(this);
        }

        /// <summary>
        /// LanguageContainer override ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Language Code: {0}, ", m_sLanguageCode3));
            sb.AppendFormat("Value: {0}, ", m_sValue);
            sb.AppendFormat("Is Default: {0}.", IsDefault);
            return sb.ToString();
        }
    }

    public class LanguageContainerComparer : IEqualityComparer<LanguageContainer>
    {
        public bool Equals(LanguageContainer lc1, LanguageContainer lc2)
        {
            return lc1.m_sLanguageCode3.Equals(lc2.m_sLanguageCode3) &&
                   lc1.m_sValue.Equals(lc2.m_sValue) &&
                   lc1.IsDefault == lc2.IsDefault;
        }

        public int GetHashCode(LanguageContainer lc)
        {
            return lc.GetHashCode();
        }
    }
}
