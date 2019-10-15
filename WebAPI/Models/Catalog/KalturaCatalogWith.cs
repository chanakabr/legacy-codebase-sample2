using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public enum KalturaCatalogWith
    {
        images,
        files,
        stats
    }

    /// <summary>
    /// Holder object for Catalog With enum
    /// </summary>    
    [Obsolete]
    public partial class KalturaCatalogWithHolder : KalturaOTTObject
    {
        [DataMember(Name="type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaCatalogWith type { get; set; }
    }
}