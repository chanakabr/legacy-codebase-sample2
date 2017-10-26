using System;

namespace ApiObjects
{
    [Serializable]
    public class LanguageContainer
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
            this.IsDefault = IsDefault;
        }

        public void Initialize(string languageCode, string value)
        {
            this.LanguageCode = languageCode;
            this.Value = value;
            this.IsDefault = false;
        }

    }
}
