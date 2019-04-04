using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
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
        internal abstract Type GetBulkUploadObjectType();
        internal abstract void Validate();
    }

    /// <summary>
    /// indicates the asset object type in the bulk file (this class is not abstract for backward-compatibility)
    /// </summary>
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

        internal override Type GetBulkUploadObjectType()
        {
            return typeof(KalturaMediaAsset);
        }
        
        internal override void Validate()
        {
            if (this.TypeId < 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "bulkUploadAssetData.typeId", 1);
            }
        }
    }

    /// <summary>
    /// indicates the media asset object type in the bulk file
    /// </summary>
    public partial class KalturaBulkUploadMediaAssetData : KalturaBulkUploadAssetData
    {
    }

    /// <summary>
    /// indicates the epg asset object type in the bulk file
    /// </summary>
    public partial class KalturaBulkUploadEpgAssetData : KalturaBulkUploadAssetData
    {
        internal override Type GetBulkUploadObjectType()
        {
            return typeof(KalturaProgramAsset);
        }

        internal override void Validate()
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
    public partial class KalturaBulkUploadMediaAssetData : KalturaBulkUploadAssetData
    {
        internal override Type GetBulkUploadObjectType()
        {
            return typeof(KalturaMediaAsset);
        }
        
        internal override void Validate()
        {
            if (this.TypeId < 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "bulkUploadAssetData.typeId", 1);
            }
        }
    }
}