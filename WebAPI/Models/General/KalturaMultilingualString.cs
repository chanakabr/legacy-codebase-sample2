using ApiObjects;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using System;
using Newtonsoft.Json.Linq;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Translated string
    /// </summary>
    public partial class KalturaMultilingualString : KalturaOTTObject
    {
        private string RequestLanguageCode;
        private string GroupDefaultLanguageCode;

        /// <summary>
        /// All values in different languages
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty("values")]
        [XmlArray(ElementName = "values", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Values { get; set; }

        #region Ctor's

        public KalturaMultilingualString(LanguageContainer[] values) : base(null)
        {
            RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            if (values == null || values.Length == 0)
            {
                List<LanguageContainer> tempValuesList = new List<LanguageContainer>();
                tempValuesList.Add(new LanguageContainer(GroupDefaultLanguageCode, string.Empty, true));
                values = tempValuesList.ToArray();
            }

            Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(values);
        }

        public KalturaMultilingualString(List<LanguageContainer> values, string defaultLanguageValue) : base(null)
        {
            RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();

            List<LanguageContainer> tempValuesList = values != null ? new List<LanguageContainer>(values) : new List<LanguageContainer>();

            if (!string.IsNullOrEmpty(GroupDefaultLanguageCode) &&
                !tempValuesList.Any(x => x.m_sLanguageCode3 == GroupDefaultLanguageCode))
            {
                tempValuesList.Add(new LanguageContainer(GroupDefaultLanguageCode, defaultLanguageValue, true));
            }

            Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(tempValuesList);
        }

        public KalturaMultilingualString(JArray values) : base(null)
        {
            RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            Values = buildList<KalturaTranslationToken>(typeof(KalturaTranslationToken), values);
        }

        public KalturaMultilingualString(List<object> values) : base(null)
        {
            RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            Values = KalturaOTTObject.buildList(typeof(KalturaTranslationToken), values.ToArray());
        }

        public KalturaMultilingualString(string defaultLanguageValue) : base(null)
        {
            RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
            GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            List<LanguageContainer> tempValuesList = new List<LanguageContainer>();
            tempValuesList.Add(new LanguageContainer(GroupDefaultLanguageCode, defaultLanguageValue, true));
            Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(tempValuesList);
        }

        #endregion

        internal List<LanguageContainer> GetLanugageContainer()
        {
            List<LanguageContainer> languageContainer = new List<LanguageContainer>();
            if (Values != null && Values.Count > 0)
            {
                foreach (KalturaTranslationToken token in Values)
                {
                    LanguageContainer lng = new LanguageContainer(token.Language, token.Value);
                    languageContainer.Add(lng);
                }
            }

            return languageContainer;
        }

        internal List<LanguageContainer> GetNoneDefaultLanugageContainer()
        {
            List<LanguageContainer> languageContainer = null;
            if (Values != null)
            {
                languageContainer = new List<LanguageContainer>();
                if (string.IsNullOrEmpty(GroupDefaultLanguageCode))
                {
                    GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                }

                foreach (KalturaTranslationToken token in Values)
                {
                    if (token.Language != GroupDefaultLanguageCode)
                    {
                        LanguageContainer lng = new LanguageContainer(token.Language, token.Value);
                        languageContainer.Add(lng);
                    }
                }
            }

            return languageContainer;
        }

        internal string GetDefaultLanugageValue()
        {
            if (string.IsNullOrEmpty(GroupDefaultLanguageCode))
            {
                GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
            }

            if (Values != null)
            {
                KalturaTranslationToken token = Values.FirstOrDefault(x => x.Language == GroupDefaultLanguageCode);
                if (token != null)
                {
                    return token.Value;
                }
            }

            return null;
        }

        public static string GetCurrent(LanguageContainer[] values, string value)
        {
            if (values != null)
            {
                return new KalturaMultilingualString(values).ToString();
            }

            return value;
        }

        public static string GetMultilingualName(string name)
        {
            return string.Format("multilingual{0}{1}", name.Substring(0, 1).ToUpper(), name.Substring(1)); ;
        }
       
        public override string ToString()
        {
            if (Values != null && Values.Count > 0)
            {
                KalturaTranslationToken token = Values.FirstOrDefault(translation => translation.Language.Equals(RequestLanguageCode));
                if (token != null)
                {
                    return token.Value;
                }

                if (GroupDefaultLanguageCode != null && !GroupDefaultLanguageCode.Equals(RequestLanguageCode))
                {
                    token = Values.FirstOrDefault(translation => translation.Language.Equals(GroupDefaultLanguageCode));
                    if (token != null)
                    {
                        return token.Value;
                    }
                }
            }

            return null;
        }

        internal void Validate(string parameterName, bool shouldCheckDefaultLanguageIsSent = true, bool shouldValidateValues = true, bool shouldValidateRequestLanguage = true,
                                bool shouldValidateLanguageExistsForGroup = true)
        {
            if (Values != null && Values.Count > 0)
            {
                HashSet<string> languageCodes = new HashSet<string>();
                HashSet<string> groupLanguageCodes = Utils.Utils.GetGroupLanguageCodes();
                if (string.IsNullOrEmpty(GroupDefaultLanguageCode))
                {
                    GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                }

                if (string.IsNullOrEmpty(RequestLanguageCode))
                {
                    RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
                }

                foreach (KalturaTranslationToken token in Values)
                {
                    if (languageCodes.Contains(token.Language))
                    {
                        throw new BadRequestException(ApiException.DUPLICATE_LANGUAGE_SENT, token.Language);
                    }

                    if (shouldValidateValues && string.IsNullOrEmpty(token.Value))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTranslationToken.value");
                    }

                        
                    if (shouldValidateLanguageExistsForGroup && !groupLanguageCodes.Contains(token.Language))
                    {
                        throw new BadRequestException(ApiException.GROUP_DOES_NOT_CONTAIN_LANGUAGE, token.Language);
                    }                    

                    languageCodes.Add(token.Language);
                }

                if (shouldCheckDefaultLanguageIsSent && !languageCodes.Contains(GroupDefaultLanguageCode))
                {
                    throw new BadRequestException(ApiException.DEFUALT_LANGUAGE_MUST_BE_SENT, parameterName);                    
                }

                if (shouldValidateRequestLanguage)
                {
                    if (string.IsNullOrEmpty(RequestLanguageCode) || RequestLanguageCode != "*")
                    {
                        throw new BadRequestException(ApiException.GLOBAL_LANGUAGE_MUST_BE_ASTERISK_FOR_WRITE_ACTIONS);
                    }
                }
            }
        }

        internal void ValidateForUpdate(string parameterName)
        {
            if (Values != null && Values.Count > 0)
            {
                HashSet<string> groupLanguageCodes = Utils.Utils.GetGroupLanguageCodes();
                if (string.IsNullOrEmpty(GroupDefaultLanguageCode))
                {
                    GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                }

                bool doesDefaultLangHasValue = Values.Any(x => x.Language == GroupDefaultLanguageCode && !string.IsNullOrEmpty(x.Value));
                if (!doesDefaultLangHasValue && Values.Any(x => x.Language != GroupDefaultLanguageCode && !string.IsNullOrEmpty(x.Value)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTranslationToken.value, you can't translate an empty value");
                }
                else
                {
                    Validate(parameterName, true, false, true, true);
                }
            }
        }
        
        public string ToCustomJson(Version currentVersion, bool omitObsolete, string propertyName)
        {
            string ret = null;
            string value = ToString();
            if (value != null)
            {
                ret = "\"" + propertyName + "\": \"" + EscapeJson(ToString()) + "\"";
            }

            string language = Utils.Utils.GetLanguageFromRequest();
            if (Values != null && language != null && language.Equals("*"))
            {
                string multilingualName = KalturaMultilingualString.GetMultilingualName(propertyName);
                if(ret != null)
                {
                    ret += ", ";
                }
                ret += "\"" + multilingualName + "\": [" + String.Join(", ", Values.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
            }
            return ret;
        }

        public string ToCustomXml(Version currentVersion, bool omitObsolete, string propertyName)
        {
            string ret = "";
            string value = ToString();
            if (value != null)
            {
                ret = "<" + propertyName + ">" + EscapeXml(ToString()) + "</" + propertyName + ">";
            }

            string language = Utils.Utils.GetLanguageFromRequest();
            if (Values != null && language != null && language.Equals("*"))
            {
                string multilingualName = KalturaMultilingualString.GetMultilingualName(propertyName);
                ret += "<" + multilingualName + "><item>" + String.Join("</item><item>", Values.Select(item => item.ToXml(currentVersion, omitObsolete))) + "</item></" + multilingualName + ">";
            }
            return ret;
        }
    }
}