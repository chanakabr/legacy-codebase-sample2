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
        public const string MEDIA_TYPE = "MEDIA_TYPE";

        [JsonProperty("TypeId")]
        public long TypeId { get; set; }

        public override IBulkUploadObject CreateObjectInstance()
        {
            var excelObject = Activator.CreateInstance(typeof(MediaAsset)) as MediaAsset;
            return excelObject;
        }
        
        public override ExcelStructure GetExcelStructure(int groupId)
        {
            var data = new Dictionary<string, object>()
            {
                { MEDIA_TYPE, TypeId }
            };
            
            return MediaAsset.GetExcelStructure(groupId, data);
        }
    }
}
