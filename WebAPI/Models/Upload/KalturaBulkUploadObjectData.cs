using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// indicates the object type in the bulk file
    /// </summary>
    public abstract partial class KalturaBulkUploadObjectData : KalturaOTTObject
    {

    }

    /// <summary>
    /// indicates the asset object type in the bulk file
    /// </summary>
    public partial class KalturaBulkUploadAssetData : KalturaBulkUploadObjectData
    {
        /// <summary>
        /// Identifies the asset type (EPG, Recording, Movie, TV Series, etc). 
        /// Possible values: 0 – EPG linear programs, 1 - Recording; or any asset type ID according to the asset types IDs defined in the system.
        /// </summary>
        [DataMember(Name = "typeId")]
        [JsonProperty(PropertyName = "typeId")]
        [XmlElement(ElementName = "typeId")]
        [SchemeProperty(MinLong = 0)]
        public long TypeId { get; set; }
    }
}