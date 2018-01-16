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
    public class KalturaChannelsFilter : KalturaFilter<KalturaChannelOrderBy>
    {
        /// <summary>
        /// Exact channel name to filter by
        /// </summary>
        [DataMember(Name = "channelEqual")]
        [JsonProperty("channelEqual")]
        [XmlElement(ElementName = "channelEqual")]
        public string ChannelEqual { get; set; }

        /// <summary>
        /// Channel starts with (autocomplete)
        /// </summary>
        [DataMember(Name = "channelStartsWith")]
        [JsonProperty("channelStartsWith")]
        [XmlElement(ElementName = "channelStartsWith")]
        public string ChannelStartsWith { get; set; }


        internal void Validate()
        {
            if (!string.IsNullOrEmpty(ChannelEqual) && !string.IsNullOrEmpty(ChannelStartsWith))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelsFilter.channelStartsWith", "KalturaChannelsFilter.channelEqual");
            }
        }

        public override KalturaChannelOrderBy GetDefaultOrderByValue()
        {
            return KalturaChannelOrderBy.NONE;
        }
    }

    public enum KalturaChannelOrderBy
    {
        NONE        
    }
}