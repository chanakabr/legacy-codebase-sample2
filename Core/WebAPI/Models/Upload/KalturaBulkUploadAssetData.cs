using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects.Base;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// indicates the asset object type in the bulk file
    /// </summary>
    [NewObjectType(typeof(KalturaBulkUploadMediaAssetData))]
    public abstract partial class KalturaBulkUploadAssetData : KalturaBulkUploadObjectData
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
