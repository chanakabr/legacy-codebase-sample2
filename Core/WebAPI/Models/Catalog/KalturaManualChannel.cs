using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaManualChannel : KalturaChannel
    {
        /// <summary>
        /// A list of comma separated media ids associated with this channel, according to the order of the medias in the channel.
        /// </summary>
        [DataMember(Name = "mediaIds")]
        [JsonProperty("mediaIds")]
        [XmlElement(ElementName = "mediaIds", IsNullable = true)]
        public string MediaIds { get; set; }

        /// <summary>
        /// List of assets identifier
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "assets", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [SchemeProperty(RequiresPermission = (int)RequestType.ALL)]
        public List<KalturaManualCollectionAsset> Assets { get; set; }
    }
}