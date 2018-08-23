using System;

namespace ApiObjects
{
    [Serializable]
    public class LanguageContainer : IEquatable<LanguageContainer>
    {

        public string LanguageCode { get; set; }
        public string Value { get; set; }
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
    }
}
