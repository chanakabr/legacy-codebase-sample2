using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{

    /// <summary>
    /// Filtering Assets requests
    /// </summary>
    [Serializable]
    [OldStandard("assets", "Assets")]
    [Obsolete]
    public class KalturaAssetsFilter : KalturaOTTObject
    {

        /// <summary>
        /// List of assets identifier
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        [XmlArray(ElementName = "assets", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaSlimAsset> Assets { get; set; }
    }
}