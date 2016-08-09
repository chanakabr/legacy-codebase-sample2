using Jil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Favorite details
    /// </summary>
    [OldStandard("extraData", "extra_data")]
    public class KalturaFavorite : KalturaOTTObject
    {
        /// <summary>
        /// AssetInfo Model
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty(PropertyName = "asset")]
        [XmlElement(ElementName = "asset", IsNullable = true)]
        [Obsolete]
        [JilDirectiveAttribute(Ignore = true)]
        public KalturaAssetInfo Asset { get; set; }

        /// <summary>
        /// AssetInfo Model
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId", IsNullable = true)]
        public long AssetId { get; set; }

        /// <summary>
        /// Extra Value
        /// </summary>
        [DataMember(Name = "extraData")]
        [JsonProperty("extraData")]
        [XmlElement(ElementName = "extraData")]
        public string ExtraData { get; set; }

        /// <summary>
        /// Specifies when was the favorite created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

    }
}