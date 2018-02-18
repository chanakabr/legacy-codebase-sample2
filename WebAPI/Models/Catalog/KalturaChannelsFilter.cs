using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaChannelsFilter : KalturaFilter<KalturaChannelsOrderBy>
    {
        /// <summary>
        /// channel identifier to filter by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        /// Exact channel name to filter by
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual")]
        public string NameEqual { get; set; }

        /// <summary>
        /// Channel name starts with (autocomplete)
        /// </summary>
        [DataMember(Name = "nameStartsWith")]
        [JsonProperty("nameStartsWith")]
        [XmlElement(ElementName = "nameStartsWith")]
        public string NameStartsWith { get; set; }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(NameEqual) && !string.IsNullOrEmpty(NameStartsWith))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelsFilter.nameStartsWith", "KalturaChannelsFilter.nameEquals");
            }

            if (!string.IsNullOrEmpty(NameStartsWith) && IdEqual > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelsFilter.nameStartsWith", "KalturaChannelsFilter.idEqual");
            }

            if (!string.IsNullOrEmpty(NameEqual) && IdEqual > 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelsFilter.nameEquals", "KalturaChannelsFilter.idEqual");
            }
        }

        public override KalturaChannelsOrderBy GetDefaultOrderByValue()
        {
            return KalturaChannelsOrderBy.NONE;
        }
    }

    public enum KalturaChannelsOrderBy
    {
        NONE,
        NAME_ASC,
        NAME_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC
    }
}