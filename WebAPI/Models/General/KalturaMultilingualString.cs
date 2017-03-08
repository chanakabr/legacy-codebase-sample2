using ApiObjects;
using ApiObjects.Pricing;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Translated string
    /// </summary>
    public class KalturaMultilingualString : KalturaOTTObject, IXmlSerializable
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

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(ToString());
        }

        public void ReadXml(XmlReader reader)
        {
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
    }
}