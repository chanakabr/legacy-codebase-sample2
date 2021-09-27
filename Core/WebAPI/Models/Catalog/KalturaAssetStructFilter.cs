using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiObjects;
using ApiObjects.Response;
using AutoMapper;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Filtering Asset Structs
    /// </summary>
    [Serializable]
    public partial class KalturaAssetStructFilter : KalturaBaseAssetStructFilter
    {
        /// <summary>
        /// Comma separated identifiers, id = 0 is identified as program AssetStruct
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        [SchemeProperty(DynamicMinInt = 0)]
        public string IdIn { get; set; }

        /// <summary>
        /// Filter Asset Structs that contain a specific meta id
        /// </summary>
        [DataMember(Name = "metaIdEqual")]
        [JsonProperty("metaIdEqual")]
        [XmlElement(ElementName = "metaIdEqual", IsNullable = true)]
        [SchemeProperty(MinLong = 1)]
        public long? MetaIdEqual { get; set; }

        /// <summary>
        /// Filter Asset Structs by isProtectedEqual value
        /// </summary>
        [DataMember(Name = "isProtectedEqual")]
        [JsonProperty("isProtectedEqual")]
        [XmlElement(ElementName = "isProtectedEqual", IsNullable = true)]
        public bool? IsProtectedEqual { get; set; }

        /// <summary>
        /// Filter Asset Structs by object virtual asset info type value
        /// </summary>
        [DataMember(Name = "objectVirtualAssetInfoTypeEqual")]
        [JsonProperty("objectVirtualAssetInfoTypeEqual")]
        [XmlElement(ElementName = "objectVirtualAssetInfoTypeEqual", IsNullable = true)]
        public KalturaObjectVirtualAssetInfoType? ObjectVirtualAssetInfoTypeEqual { get; set; }

        private List<long> AssetStructIds { get; set; }

        internal override void Validate()
        {
            if (!string.IsNullOrEmpty(IdIn) && MetaIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaAssetStructFilter.idIn", "KalturaAssetStructFilter.metaIdEqual");
            }

            if (!string.IsNullOrEmpty(IdIn))
            {
                // GetAssetStructIds does parsing and validation (throws BadRequest).
                // Ideally it should be just validation here, but as result is available then we could save and use it.
                AssetStructIds = GetAssetStructIds();
            }
        }

        internal override GenericListResponse<AssetStruct> GetResponse(int groupId)
        {
            if (MetaIdEqual > 0)
            {
                return CatalogManager.Instance.GetAssetStructsByTopicId(groupId, MetaIdEqual.Value, IsProtectedEqual);
            }

            if (ObjectVirtualAssetInfoTypeEqual.HasValue)
            {
                var virtualEntityType = Mapper.Map<ObjectVirtualAssetInfoType>(ObjectVirtualAssetInfoTypeEqual);

                return CatalogManager.Instance.GetAssetStructByVirtualEntityType(groupId, virtualEntityType);
            }

            var assetStructIds = AssetStructIds ?? GetAssetStructIds();

            return CatalogManager.Instance.GetAssetStructsByIds(groupId, assetStructIds, IsProtectedEqual);
        }

        private List<long> GetAssetStructIds()
            => GetItemsIn<List<long>, long>(IdIn, "KalturaAssetStructFilter.idIn", checkDuplicate: true, ignoreDefaultValueValidation: true);
    }
}