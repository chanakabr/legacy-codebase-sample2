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
    /// indicates the object type in the bulk file
    /// </summary>
    public abstract partial class KalturaBulkUploadObjectData : KalturaOTTObject
    {
        internal abstract string GetBulkUploadObjectType();
        internal abstract void Validate(int groupId);
    }

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

    /// <summary>
    /// indicates the media asset object type in the bulk file
    /// </summary>
    public partial class KalturaBulkUploadMediaAssetData : KalturaBulkUploadAssetData
    {
        private static readonly string bulkUploadObjectType = typeof(KalturaMediaAsset).Name;

        internal override string GetBulkUploadObjectType()
        {
            return bulkUploadObjectType;
        }

        internal override void Validate(int groupId)
        {
            if (this.TypeId < 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "bulkUploadAssetData.typeId", 1);
            }
        }
    }

    /// <summary>
    /// indicates the epg asset object type in the bulk file
    /// </summary>
    public partial class KalturaBulkUploadProgramAssetData : KalturaBulkUploadAssetData
    {
        private static readonly string bulkUploadObjectType = typeof(KalturaProgramAsset).Name;

        internal override string GetBulkUploadObjectType()
        {
            return bulkUploadObjectType;
        }

        internal override void Validate(int groupId)
        {
            if (this.TypeId != 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "bulkUploadAssetData.typeId");
            }
        }
    }

    /// <summary>
    /// indicates the media asset object type in the bulk file
    /// </summary>
    public partial class KalturaBulkUploadLiveAssetData : KalturaBulkUploadMediaAssetData
    {
        private static readonly string bulkUploadObjectType = typeof(KalturaLiveAsset).Name;

        internal override string GetBulkUploadObjectType()
        {
            return bulkUploadObjectType;
        }

        internal override void Validate(int groupId)
        {
            var linearMediaTypeIds = CatalogManager.GetLinearMediaTypeIds(groupId);
            if (!linearMediaTypeIds.Contains(this.TypeId))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "bulkUploadAssetData.typeId");
            }
        }
    }

    // TODO SHIR -BULKDATA FOR UDID
}