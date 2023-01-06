using MongoDB.Bson.Serialization.Attributes;
using OTT.Lib.MongoDB;

namespace IngestHandler.Common.Repositories.Models
{
    [MongoDbIgnoreExternalElements]
    public class BulkUploadIdempotencyDocument
    {
        [BsonId]
        public string Id { get; }

        public long BulkUploadId { get; }

        public long LinearChannelId { get; }

        [BsonConstructor]
        public BulkUploadIdempotencyDocument(long bulkUploadId, long linearChannelId)
        {
            Id = $"{bulkUploadId}_{linearChannelId}";
            BulkUploadId = bulkUploadId;
            LinearChannelId = linearChannelId;
        }
    }
}