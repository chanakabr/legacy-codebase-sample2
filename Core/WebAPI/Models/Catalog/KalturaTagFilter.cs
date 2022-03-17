using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaTagFilter : KalturaFilter<KalturaTagOrderBy>
    {
        /// <summary>
        /// Tag to filter by
        /// </summary>
        [DataMember(Name = "tagEqual")]
        [JsonProperty("tagEqual")]
        [XmlElement(ElementName = "tagEqual")]
        public string TagEqual { get; set; }

        /// <summary>
        /// Tag to filter by
        /// </summary>
        [DataMember(Name = "tagStartsWith")]
        [JsonProperty("tagStartsWith")]
        [XmlElement(ElementName = "tagStartsWith")]
        public string TagStartsWith { get; set; }

        /// <summary>
        /// Type identifier
        /// </summary>
        [DataMember(Name = "typeEqual")]
        [JsonProperty("typeEqual")]
        [XmlElement(ElementName = "typeEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int? TypeEqual { get; set; }

        /// <summary>
        /// Language to filter by
        /// </summary>
        [DataMember(Name = "languageEqual")]
        [JsonProperty("languageEqual")]
        [XmlElement(ElementName = "languageEqual")]        
        public string LanguageEqual { get; set; }

        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }

        public override KalturaTagOrderBy GetDefaultOrderByValue()
        {
            return KalturaTagOrderBy.NONE;
        }
    }
}