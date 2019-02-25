using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// Bulk Upload Result
    /// </summary>
    [Serializable]
    public abstract partial class KalturaBulkUploadResult : KalturaOTTObject
    {
        /// <summary>
        /// the result ObjectId (assetId, userId etc)
        /// </summary>
        [DataMember(Name = "objectId")]
        [JsonProperty("objectId")]
        [XmlElement(ElementName = "objectId")]
        [SchemeProperty(ReadOnly = true)]
        public long ObjectId { get; set; }

        /// <summary>
        /// result index
        /// </summary>
        [DataMember(Name = "index")]
        [JsonProperty("index")]
        [XmlElement(ElementName = "index")]
        [SchemeProperty(ReadOnly = true)]
        public int Index { get; set; }

        /// <summary>
        /// Bulk upload identifier
        /// </summary>
        [DataMember(Name = "bulkUploadId")]
        [JsonProperty("bulkUploadId")]
        [XmlElement(ElementName = "bulkUploadId")]
        [SchemeProperty(ReadOnly = true)]
        public long BulkUploadId { get; set; }

        /// <summary>
        /// status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaResponseStatus Status { get; set; }
    }

    public partial class KalturaBulkUploadAssetResult : KalturaBulkUploadResult
    {
        /// <summary>
        /// Identifies the asset type (EPG, Recording, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, 1 - Recording; or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type")]
        [SchemeProperty(ReadOnly = true)]
        public int Type { get; set; }

        /// <summary>
        /// External identifier for the asset
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty(PropertyName = "externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(ReadOnly = true)]
        public string ExternalId { get; set; }
    }
}