using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class LanguageContainer : IEquatable<LanguageContainer>
    {
        [DataMember]
        [JsonProperty("LanguageCode")]
        public string LanguageCode { get; set; }

        [DataMember]
        [JsonProperty("Value")]
        public string Value { get; set; }

        [DataMember]
        [JsonProperty("IsDefault")]
        public bool IsDefault { get; set; }

        public LanguageContainer()
        {
            this.LanguageCode = string.Empty;
            this.Value = string.Empty;
            this.IsDefault = false;
        }

        public LanguageContainer(string languageCode, string value)
        {
            this.LanguageCode = languageCode;
            this.Value = value;
            this.IsDefault = false;
        }

        public LanguageContainer(string languageCode, string value, bool isDefault)
        {
            this.LanguageCode = languageCode;
            this.Value = value;
            this.IsDefault = isDefault;
        }

        public void Initialize(string languageCode, string value)
        {
            this.LanguageCode = languageCode;
            this.Value = value;
            this.IsDefault = false;
        }

        public bool Equals(LanguageContainer other)
        {
            if (other == null)
                return false;

            return LanguageCode.Equals(other.LanguageCode) &&
                Value.Equals(other.Value) &&
                IsDefault == other.IsDefault;
        }

        /// <summary>
        /// LanguageContainer override ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(string.Format("Language Code: {0}, ", LanguageCode));
            sb.AppendFormat("Value: {0}, ", Value);
            sb.AppendFormat("Is Default: {0}.", IsDefault);
            return sb.ToString();
        }
    }

    public class LanguageContainerComparer : IEqualityComparer<LanguageContainer>
    {
        public bool Equals(LanguageContainer lc1, LanguageContainer lc2)
        {
            return lc1.LanguageCode.Equals(lc2.LanguageCode) &&
                   lc1.Value.Equals(lc2.Value) &&
                   lc1.IsDefault == lc2.IsDefault;
        }

        public int GetHashCode(LanguageContainer lc)
        {
            return lc.GetHashCode();
        }
    }
}
