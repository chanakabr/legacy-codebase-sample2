using System;
using MongoDB.Bson.Serialization.Attributes;

namespace LiveToVod.DAL
{
    internal class LiveToVodLinearAssetConfigurationData
    {
        [BsonId]
        public long LinearAssetId { get; set; }

        public bool IsLiveToVodEnabled { get; set; }

        public int? RetentionPeriodDays { get; set; }

        public long LastUpdaterId { get; set; }
        
        [BsonElement("__updated")]
        public DateTime UpdateDate { get; set; }
    }
}