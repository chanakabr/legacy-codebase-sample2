using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Category details
    /// </summary>
    public class KalturaOTTCategory : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the category
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

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
        [DataMember(Name = "parentCategoryId")]
        [JsonProperty(PropertyName = "parentCategoryId")]
        [XmlElement(ElementName = "parentCategoryId")]
        [OldStandardProperty("parent_category_id")]
        public long? ParentCategoryId { get; set; }

        /// <summary>
        /// Child categories 
        /// </summary>
        [DataMember(Name = "childCategories")]
        [JsonProperty(PropertyName = "childCategories")]
        [XmlArray(ElementName = "childCategories", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("child_categories")]
        public List<KalturaOTTCategory> ChildCategories { get; set; }

        /// <summary>
        /// Category channels
        /// </summary>
        [DataMember(Name = "channels")]
        [JsonProperty(PropertyName = "channels")]
        [XmlArray(ElementName = "channels", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaChannel> Channels { get; set; }

        /// <summary>
        /// Category images
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaMediaImage> Images { get; set; }
    }
}