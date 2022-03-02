using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// ProgramAssetGroupOffer list
    /// </summary>
    [DataContract(Name = "ProgramAssetGroupOffers", Namespace = "")]
    [XmlRoot("ProgramAssetGroupOffers")]
    public partial class KalturaProgramAssetGroupOfferListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of collections
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaProgramAssetGroupOffer> Objects { get; set; }
    }
}