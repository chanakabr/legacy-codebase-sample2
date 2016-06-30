using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public enum KalturaCatalogWith
    {
        IMAGES,
        FILES,
        STATS
    }

    /// <summary>
    /// Holder object for Catalog With enum
    /// </summary>    
    public class KalturaCatalogWithHolder : KalturaOTTObject
    {
        [DataMember(Name="type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaCatalogWith type { get; set; }
    }
}