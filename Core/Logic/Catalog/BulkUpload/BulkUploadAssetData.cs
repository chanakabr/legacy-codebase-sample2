using ApiObjects.BulkUpload;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Managers;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadAssetData : BulkUploadObjectData
    {
        private AssetStructStructureManager structureManager { get; set; }

        [JsonProperty("TypeId")]
        public long TypeId { get; set; }

        public override IBulkUploadStructureManager GetStructureManager()
        {
            if (structureManager == null)
            {
                var assetStructResponse = CatalogManager.Instance.GetAssetStruct(GroupId, TypeId);
                if (assetStructResponse.HasObject())
                {
                    var assetStruct = assetStructResponse.Object;
                    if (assetStruct.TopicsMapBySystemName == null || assetStruct.TopicsMapBySystemName.Count == 0)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(GroupId, out catalogGroupCache))
                        {
                            assetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                                                              .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key))
                                                              .ToDictionary(x => x.Value.SystemName, y => y.Value);
                        }
                    }

                    structureManager = new AssetStructStructureManager(assetStruct);
                }
            }

            return structureManager;
        }
    }
}