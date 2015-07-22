using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Slim assets wrapper
    /// </summary>
    public class SlimAssetInfoWrapper : BaseListWrapper
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "assets")]
        [JsonProperty(PropertyName = "assets")]
        public List<KalturaSlimAssetInfo> Assets { get; set; }
    }

    /// <summary>
    /// Slim asset info
    /// </summary>
    public class KalturaSlimAssetInfo : KalturaIAssetable
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Identifies the asset type (EPG, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        /// <summary>
        /// Asset name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Asset description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Collection of images details that can be used to represent this asset
        /// </summary>
        [DataMember(Name = "images", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "images", NullValueHandling = NullValueHandling.Ignore)]
        public List<KalturaImage> Images { get; set; }

        /// <summary>
        /// Files
        /// </summary>
        [DataMember(Name = "files", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "files", NullValueHandling = NullValueHandling.Ignore)]
        public List<KalturaFile> Files { get; set; }

        /// <summary>
        /// Collection of add-on statistical information for the media. See AssetStats model for more information
        /// </summary>
        [DataMember(Name = "stats", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        public KalturaAssetStats Statistics { get; set; }
      
    }
}