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
    /// Category details
    /// </summary>
    public class KalturaCategory : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Category parent identifier 
        /// </summary>
        [DataMember(Name = "parent_category_id")]
        [JsonProperty(PropertyName = "parent_category_id")]
        public long ParentCategoryId { get; set; }

        /// <summary>
        /// Child categories 
        /// </summary>
        [DataMember(Name = "child_categories")]
        [JsonProperty(PropertyName = "child_categories")]
        public List<KalturaCategory> ChildCategories { get; set; }

        /// <summary>
        /// Category channels
        /// </summary>
        [DataMember(Name = "channels")]
        [JsonProperty(PropertyName = "channels")]
        public List<KalturaChannel> Channels { get; set; }

        /// <summary>
        /// Category images
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        public List<KalturaImage> Images { get; set; }
    }
}