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
        [JsonProperty("TypeId")]
        public long TypeId { get; set; }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var excelObject = Activator.CreateInstance(typeof(MediaAsset)) as MediaAsset;
            return excelObject;
        }
        
        public override IBulkUploadStructure GetStructure()
        {
            AssetStruct assetStruct = new AssetStruct
            {
                Id = TypeId
            };

            return assetStruct;
        }
    }
}
