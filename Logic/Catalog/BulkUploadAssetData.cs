using ApiObjects.BulkUpload;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
                var assetStructResponse = CatalogManagement.CatalogManager.GetAssetStruct(GroupId, TypeId);
                if (assetStructResponse.HasObject())
                {
                    structure = assetStructResponse.Object;
                }
            }
            
            return structure;
        }
        
        public override bool Validate(Dictionary<string, object> propertyToValueMap)
        {
            // set structure if null
            GetStructure();

            if (structure != null )
            {
                var mediaAssetTypeColumnName = ExcelColumn.GetFullColumnName(MediaAsset.MEDIA_ASSET_TYPE, null, null, true);
                if (propertyToValueMap.ContainsKey(mediaAssetTypeColumnName) &&
                    propertyToValueMap[mediaAssetTypeColumnName].ToString().Equals(structure.SystemName))
                {
                    return true;
                }
            }
            
            return false;
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
