using ApiObjects.BulkUpload;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadAssetData : BulkUploadObjectData
    {
        private AssetStruct structure { get; set; }

        [JsonProperty("TypeId")]
        public long TypeId { get; set; }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var excelObject = Activator.CreateInstance(typeof(MediaAsset)) as MediaAsset;
            return excelObject;
        }

        public override IBulkUploadStructure GetStructure()
        {
            if (structure == null)
            {
                var assetStructResponse = CatalogManager.GetAssetStruct(GroupId, TypeId);
                if (assetStructResponse.HasObject())
                {
                    structure = assetStructResponse.Object;
                    if (structure.TopicsMapBySystemName == null || structure.TopicsMapBySystemName.Count == 0)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (CatalogManager.TryGetCatalogGroupCacheFromCache(GroupId, out catalogGroupCache))
                        {
                            structure.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => structure.MetaIds.Contains(x.Key))
                                                              .OrderBy(x => structure.MetaIds.IndexOf(x.Key))
                                                              .ToDictionary(x => x.Value.SystemName, y => y.Value);
                        }
                    }
                }
            }

            return structure;
        }

        public override bool Validate(Dictionary<string, object> propertyToValueMap)
        {
            var mandatoryPropertyToValueMap = GetMandatoryPropertyToValueMap();
            foreach (var mandatoryPropertyToValue in mandatoryPropertyToValueMap)
            {
                if (!propertyToValueMap.ContainsKey(mandatoryPropertyToValue.Key) || 
                    !propertyToValueMap[mandatoryPropertyToValue.Key].Equals(mandatoryPropertyToValue.Value))
                {
                    return false;
                }
            }

            return true;
        }

        public override Dictionary<string, object> GetMandatoryPropertyToValueMap()
        {
            Dictionary<string, object> mandatoryPropertyToValueMap = new Dictionary<string, object>();
            // set structure if null
            GetStructure();

            if (structure != null)
            {
                var mediaAssetTypeColumnName = ExcelColumn.GetFullColumnName(MediaAsset.MEDIA_ASSET_TYPE, null, null, true);
                mandatoryPropertyToValueMap.Add(mediaAssetTypeColumnName, structure.SystemName);
            }

            return mandatoryPropertyToValueMap;
        }
    }
}