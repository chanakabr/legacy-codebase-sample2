using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaLinearChannel : KalturaOTTObject
    {
        /// <summary>
        /// The identifier of the linear media representing the channel
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public int Id { get; set; }

        /// <summary>
        /// The number of the channel
        /// </summary>
        [DataMember(Name = "channelNumber")]
        [JsonProperty("channelNumber")]
        [XmlElement(ElementName = "channelNumber")]
        public int ChannelNumber { get; set; }
    }
}