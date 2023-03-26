using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// A Lineup channel asset is KalturaLiveAsset in a context of specific region (includes LCN)
    /// </summary>
    public partial class KalturaLineupChannelAsset : KalturaLiveAsset
    {
        /// <summary>
        /// Lineup channel number (LCN) - A logical linear channel number. This number is unique in the region context.
        /// </summary>
        [DataMember(Name = "lcn")]
        [JsonProperty(PropertyName = "lcn")]
        [XmlElement(ElementName = "lcn")]
        public int? LinearChannelNumber { get; set; }
    }
}