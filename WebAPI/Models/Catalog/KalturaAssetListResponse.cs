using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Asset wrapper
    /// </summary>
    [Serializable]
    public partial class KalturaAssetListResponse : KalturaListResponse, IKalturaExcelableListResponse
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAsset> Objects { get; set; }

        public ExcelStructure GetExcelStructure(int groupId)
        {
            if (Objects == null || Objects.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "KalturaAssetListResponse.objects");
            }

            var duplicates = Objects.GroupBy(x => x.getType()).Select(x => x.Key).ToList();
            if (duplicates.Count > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER,
                                              "KalturaAsset.type:" + duplicates[0].ToString(),
                                              "KalturaAsset.type:" + duplicates[1].ToString());
            }

            KalturaAssetStruct kalturaAssetStruct = new KalturaAssetStruct()
            {
                Id = duplicates[0]
            };

            ExcelStructure excelStructer = kalturaAssetStruct.GetExcelStructure(groupId);
            return excelStructer;
        }

        public List<IKalturaExcelableObject> GetObjects()
        {
            return this.Objects.ToList<IKalturaExcelableObject>();
        }
    }
}