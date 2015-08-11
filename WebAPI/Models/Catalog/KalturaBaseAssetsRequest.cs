using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Base assets request parameters
    /// </summary>
    [Serializable]
    public class KalturaBaseAssetsRequest : KalturaOTTObject
    {
        /// <summary>
        /// Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        [XmlArray(ElementName = "with")]
        [XmlArrayItem("item")] 
        public List<KalturaCatalogWith> with { get; set; }
    }
}