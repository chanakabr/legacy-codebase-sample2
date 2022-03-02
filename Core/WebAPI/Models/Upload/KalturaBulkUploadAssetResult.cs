using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    [Serializable]
    public abstract partial class KalturaBulkUploadAssetResult : KalturaBulkUploadResult
    {
        /// <summary>
        /// Identifies the asset type (EPG, Recording, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, 1 - Recording; or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "type")]
        [JsonProperty(PropertyName = "type")]
        [XmlElement(ElementName = "type", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public int? Type { get; set; }

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
