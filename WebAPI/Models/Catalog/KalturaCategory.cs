using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
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
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// Category name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Category parent identifier 
        /// </summary>
        [DataMember(Name = "parent_category_id")]
        [JsonProperty(PropertyName = "parent_category_id")]
        [XmlElement(ElementName = "parent_category_id")]
        public long ParentCategoryId { get; set; }

        /// <summary>
        /// Child categories 
        /// </summary>
        [DataMember(Name = "child_categories")]
        [JsonProperty(PropertyName = "child_categories")]
        [XmlArray(ElementName = "child_categories")]
        [XmlArrayItem("item")] 
        public List<KalturaCategory> ChildCategories { get; set; }

        /// <summary>
        /// Category channels
        /// </summary>
        [DataMember(Name = "channels")]
        [JsonProperty(PropertyName = "channels")]
        [XmlArray(ElementName = "channels")]
        [XmlArrayItem("item")] 
        public List<KalturaChannel> Channels { get; set; }

        /// <summary>
        /// Category images
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images")]
        [XmlArrayItem("item")] 
        public List<KalturaImage> Images { get; set; }
    }
}