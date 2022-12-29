using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// 
    /// </summary>
    public partial class KalturaObjectVirtualAssetPartnerConfig : KalturaPartnerConfiguration
    {   
        /// <summary>
        ///List of object virtual asset info
        /// </summary>
        [DataMember(Name = "objectVirtualAssets")]
        [JsonProperty("objectVirtualAssets")]
        [XmlElement(ElementName = "objectVirtualAssets")]
        public List<KalturaObjectVirtualAssetInfo> ObjectVirtualAssets { get; set; }
    }
}