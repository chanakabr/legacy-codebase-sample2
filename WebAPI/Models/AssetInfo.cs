using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    public class AssetInfo
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Images
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        public List<Image> Images { get; set; }

        /// <summary>
        /// Files
        /// </summary>
        [DataMember(Name = "files", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "files", NullValueHandling = NullValueHandling.Ignore)]
        public List<File> Files { get; set; }

        /// <summary>
        /// Metas
        /// </summary>
        [DataMember(Name = "metas")]
        [JsonProperty(PropertyName = "metas")]
        public Dictionary<string, string> Metas { get; set; }

        /// <summary>
        /// Tags
        /// </summary>
        [DataMember(Name = "tags")]
        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, List<string>> Tags { get; set; }

        /// <summary>
        /// Start date
        /// </summary>
        [DataMember(Name = "start_date")]
        [JsonProperty(PropertyName = "start_date")]
        public long StartDate { get; set; }

        /// <summary>
        /// End date
        /// </summary>
        [DataMember(Name = "end_date")]
        [JsonProperty(PropertyName = "end_date")]
        public long EndDate { get; set; }

        /// <summary>
        /// Asset statistics
        /// </summary>
        [DataMember(Name = "stats", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        public AssetStats Statistics { get; set; }

        /// <summary>
        /// Extra parameters
        /// </summary>
        [DataMember(Name = "extra_params", EmitDefaultValue = true)]
        [JsonProperty(PropertyName = "extra_params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> ExtraParams { get; set; }
    }
}