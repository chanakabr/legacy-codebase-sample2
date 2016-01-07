using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using ApiObjects;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPApiModule.Objects.Responses
{
    public class PersonalAssetRequest
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
        /// Identifies the asset type (EPG, Media, etc). 
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        public eAssetTypes Type
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
        public List<long> FileIds
        {
            get;
            set;
        }
    }
}
