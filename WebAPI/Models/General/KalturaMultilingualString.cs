using ApiObjects;
using ApiObjects.Pricing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using WebAPI.App_Start;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Translated string
    /// </summary>
    public partial class KalturaMultilingualString : KalturaOTTObject
    {
        private string language;
        private string defaultLanguage;

        public KalturaMultilingualString()
        {
        }

        public KalturaMultilingualString(LanguageContainer[] values)
        {
            language = Utils.Utils.GetLanguageFromRequest();
            defaultLanguage = Utils.Utils.GetDefaultLanguage();

            Values = AutoMapper.Mapper.Map<List<KalturaTranslationToken>>(values);
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
        
        /// <summary>
        /// All values in different languages
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty("values")]
        [XmlArray(ElementName = "values", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaTranslationToken> Values { get; set; }

        public override string ToString()
        {
            if(Values != null && Values.Count > 0)
            {
                KalturaTranslationToken token;
                IEnumerable<KalturaTranslationToken> tokens = Values.Where(translation => translation.Language.Equals(language));
                if (tokens != null && tokens.Count() > 0)
                {
                    token = tokens.First();
                    if (token != null)
                    {
                        return token.Value;
                    }
                }

                if (defaultLanguage != null && !defaultLanguage.Equals(language))
                {
                    tokens = Values.Where(translation => translation.Language.Equals(defaultLanguage));
                    if (tokens != null && tokens.Count() > 0)
                    {
                        token = tokens.First();
                        if (token != null)
                        {
                            return token.Value;
                        }
                    }
                }
            }

            return null;
        }

        public string ToCustomJson(Version currentVersion, bool omitObsolete, string propertyName)
        {
            string ret = "\"" + propertyName + "\": \"" + ToString() + "\"";

            string language = Utils.Utils.GetLanguageFromRequest();
            if (Values != null && language != null && language.Equals("*"))
            {
                string multilingualName = KalturaMultilingualString.GetMultilingualName(propertyName);
                ret += ", \"" + multilingualName + "\": [" + String.Join(", ", Values.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
            }
            return ret;
        }

        public string ToCustomXml(Version currentVersion, bool omitObsolete, string propertyName)
        {
            string ret = "<" + propertyName + ">" + ToString() + "</" + propertyName + ">";

            string language = Utils.Utils.GetLanguageFromRequest();
            if (Values != null && language != null && language.Equals("*"))
            {
                string multilingualName = KalturaMultilingualString.GetMultilingualName(propertyName);
                ret += "<" + multilingualName + "><item>" + String.Join("</item><item>", Values.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item></" + multilingualName + ">";
            }
            return ret;
        }
    }
}