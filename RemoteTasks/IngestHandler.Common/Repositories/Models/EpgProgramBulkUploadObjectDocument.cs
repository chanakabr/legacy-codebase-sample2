using ApiObjects.BulkUpload;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OTT.Lib.MongoDB;

namespace IngestHandler.Common.Repositories.Models
{
    public enum BulkUploadCRUDOperation
    {
        Added,
        Deleted,
        Updated,
        Affected,
    }
    
    [MongoDbIgnoreExternalElements]
    public class EpgProgramBulkUploadObjectDocument
    {
        public long BulkUploadId { get; set; }
        public EpgProgramBulkUploadObject EpgProgramBulkUploadObject { get; set; }
            
        [JsonConverter(typeof(StringEnumConverter))] 
        [BsonRepresentation(BsonType.String)]
        public BulkUploadCRUDOperation Operation { get; set; }
            
        public EpgProgramBulkUploadObjectDocument(long bulkUploadId, EpgProgramBulkUploadObject epgProgramBulkUploadObject, BulkUploadCRUDOperation op)
        {
            EpgProgramBulkUploadObject = epgProgramBulkUploadObject;
            BulkUploadId = bulkUploadId;
            Operation = op;
        }
    }
}