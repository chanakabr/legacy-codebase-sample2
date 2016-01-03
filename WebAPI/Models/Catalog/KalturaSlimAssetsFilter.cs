using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    /// <summary>
    /// Filtering Assets requests
    /// </summary>
    [Serializable]
    public class KalturaSlimAssetsFilter : KalturaOTTObject
    {

        /// <summary>
        /// Assets identifier
        /// </summary>
        [DataMember(Name = "Assets")]
        [JsonProperty(PropertyName = "Assets")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaSlimAsset> Assets { get; set; }

        /// <summary>
        /// Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By { get; set; }
    }
}