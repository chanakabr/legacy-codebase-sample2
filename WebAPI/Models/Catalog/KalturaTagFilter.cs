using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaTagFilter : KalturaFilter<KalturaTagOrderBy>
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
        public int TypeEqual { get; set; }

        internal void Validate()
        {
            if (this.TypeEqual <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTagFilter.typeEqual");
            }

            if (!string.IsNullOrEmpty(TagEqual) && !string.IsNullOrEmpty(TagStartsWith))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaTagFilter.tagEqual", "KalturaTagFilter.tagStartsWith");
            }
        }

        public override KalturaTagOrderBy GetDefaultOrderByValue()
        {
            return KalturaTagOrderBy.NONE;
        }


    }

    public enum KalturaTagOrderBy
    {
        NONE
    }
}