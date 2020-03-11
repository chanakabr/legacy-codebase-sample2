using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaObjectVirtualAssetInfo : KalturaOTTObject
    {
        /// <summary>
        /// Asset struct identifier
        /// </summary>
        [DataMember(Name = "assetStructId")]
        [JsonProperty("assetStructId")]
        [XmlElement(ElementName = "assetStructId")]
        public int AssetStructId { get; set; }


        /// <summary>
        /// Meta identifier
        /// </summary>
        [DataMember(Name = "metaId")]
        [JsonProperty("metaId")]
        [XmlElement(ElementName = "metaId")]
        public int MetaId { get; set; }

        /// <summary>
        /// Object virtual asset info type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement(ElementName = "type")]
        public KalturaObjectVirtualAssetInfoType Type { get; set; }
    }

    public enum KalturaObjectVirtualAssetInfoType { Subscription = 0, Segment = 1, Category = 2 }

}