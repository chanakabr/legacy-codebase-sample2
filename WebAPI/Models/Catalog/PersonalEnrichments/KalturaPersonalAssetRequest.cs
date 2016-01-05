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
    public class KalturaPersonalAssetRequest : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies the asset type (EPG, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public int Type
        {
            get;
            set;
        }

        /// <summary>
        /// Files of the current asset
        /// </summary>
        [DataMember(Name = "file_ids")]
        [JsonProperty(PropertyName = "file_ids")]
        [XmlArray(ElementName = "file_ids", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaLongValue> FileIds
        {
            get;
            set;
        }
    }
}