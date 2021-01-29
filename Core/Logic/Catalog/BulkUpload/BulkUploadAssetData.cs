using ApiObjects.BulkUpload;
using Core.Catalog.CatalogManagement;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Core.Catalog
{
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadAssetData : BulkUploadObjectData
    {
        protected AssetStruct structureManager { get; private set; }

        [JsonProperty("TypeId")]
        public long TypeId { get; set; }

        public override IBulkUploadStructureManager GetStructureManager()
        {
            if (structureManager == null)
            {
                var assetStructResponse = CatalogManager.Instance.GetAssetStruct(GroupId, TypeId);
                if (assetStructResponse.HasObject())
                {
                    structureManager = assetStructResponse.Object;
                    if (structureManager.TopicsMapBySystemName == null || structureManager.TopicsMapBySystemName.Count == 0)
                    {
                        CatalogGroupCache catalogGroupCache;
                        if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(GroupId, out catalogGroupCache))
                        {
                            structureManager.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => structureManager.MetaIds.Contains(x.Key))
                                                              .OrderBy(x => structureManager.MetaIds.IndexOf(x.Key))
                                                              .ToDictionary(x => x.Value.SystemName, y => y.Value);
                        }
                    }
                }
            }

            return structureManager;
        }
    }
}