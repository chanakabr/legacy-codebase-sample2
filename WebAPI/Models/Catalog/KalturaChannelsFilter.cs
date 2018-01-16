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
        /// Channel to filter by
        /// </summary>
        [DataMember(Name = "channelEqual")]
        [JsonProperty("channelEqual")]
        [XmlElement(ElementName = "channelEqual")]
        public string ChannelEqual { get; set; }

        /// <summary>
        /// Channel like
        /// </summary>
        [DataMember(Name = "channelLike")]
        [JsonProperty("channelLike")]
        [XmlElement(ElementName = "channelLike")]
        public string ChannelLike { get; set; }


        internal void Validate()
        {
            if (!string.IsNullOrEmpty(ChannelEqual) && !string.IsNullOrEmpty(ChannelLike))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaChannelsFilter.channelLike", "KalturaChannelsFilter.channelEqual");
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