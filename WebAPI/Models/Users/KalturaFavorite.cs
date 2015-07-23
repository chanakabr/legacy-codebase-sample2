using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Favorite details
    /// </summary>
    public class KalturaFavorite : KalturaOTTObject
    {
        /// <summary>
        /// AssetInfo Model
        /// </summary>
        [DataMember(Name = "asset")]
        [JsonProperty(PropertyName = "asset")]
        public KalturaAssetInfo Asset { get; set; }

        /// <summary>
        /// Extra Value
        /// </summary>
        [DataMember(Name = "extra_data")]
        [JsonProperty("extra_data")]
        public string ExtraData { get; set; }

    }
}