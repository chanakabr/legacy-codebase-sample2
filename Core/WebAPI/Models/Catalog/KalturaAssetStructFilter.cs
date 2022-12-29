using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering Asset Structs
    /// </summary>
    [Serializable]
    public partial class KalturaAssetStructFilter : KalturaBaseAssetStructFilter
    {
        /// <summary>
        /// Comma separated identifiers, id = 0 is identified as program AssetStruct
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        /// <summary>
        /// Filter Asset Structs that contain a specific meta id
        /// </summary>
        [DataMember(Name = "metaIdEqual")]
        [JsonProperty("metaIdEqual")]
        [XmlElement(ElementName = "metaIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? MetaIdEqual { get; set; }

        /// <summary>
        /// Filter Asset Structs by isProtectedEqual value
        /// </summary>
        [DataMember(Name = "isProtectedEqual")]
        [JsonProperty("isProtectedEqual")]
        [XmlElement(ElementName = "isProtectedEqual", IsNullable = true)]
        public bool? IsProtectedEqual { get; set; }

        /// <summary>
        /// Filter Asset Structs by object virtual asset info type value
        /// </summary>
        [DataMember(Name = "objectVirtualAssetInfoTypeEqual")]
        [JsonProperty("objectVirtualAssetInfoTypeEqual")]
        [XmlElement(ElementName = "objectVirtualAssetInfoTypeEqual", IsNullable = true)]
        public KalturaObjectVirtualAssetInfoType? ObjectVirtualAssetInfoTypeEqual { get; set; }
    }
}