using System;

namespace ApiObjects
{
    [Serializable]
    public class LanguageContainer
    {
        public LanguageContainer()
        {
            m_sLanguageCode3 = string.Empty;
            m_sValue = string.Empty;
        }
        public void Initialize(string sLanguageCode, string sValue)
        {
            m_sLanguageCode3 = sLanguageCode;
            m_sValue = sValue;
        }

        public string m_sLanguageCode3;
        public string m_sValue;
    }
}
