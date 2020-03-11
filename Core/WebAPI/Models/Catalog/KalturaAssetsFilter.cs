using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    /// <summary>
    /// Filtering Assets requests
    /// </summary>
    [Serializable]
    [Obsolete]
    public partial class KalturaAssetsFilter : KalturaOTTObject
    {

        /// <summary>
        /// List of assets identifier
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "assets", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [OldStandardProperty("Assets")]
        public List<KalturaSlimAsset> Assets { get; set; }
    }
}