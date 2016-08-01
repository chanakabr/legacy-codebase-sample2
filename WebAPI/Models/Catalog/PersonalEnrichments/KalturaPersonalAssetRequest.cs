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
    [OldStandard("fileIds", "file_ids")]
    [Obsolete]
    public class KalturaPersonalAssetRequest : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the asset
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public long? Id
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
        public KalturaAssetType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Files of the current asset
        /// </summary>
        [DataMember(Name = "fileIds")]
        [JsonProperty(PropertyName = "fileIds")]
        [XmlArray(ElementName = "fileIds", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<long> FileIds
        {
            get;
            set;
        }

        internal long getId()
        {
            return Id.HasValue ? (long)Id : 0;
        }
    }
}