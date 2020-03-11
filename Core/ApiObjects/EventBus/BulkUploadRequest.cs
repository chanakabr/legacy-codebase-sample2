using EventBus.Abstraction;
using Newtonsoft.Json;
using System;

namespace ApiObjects.EventBus
{
    [Serializable]
    public class BulkUploadRequest : ServiceEvent
    {
        [JsonProperty("bulk_upload_id")]
        public long BulkUploadId { get; set; }

        [JsonProperty("user_id")]
        public long UserId { get; set; }

        public override string ToString()
        {
            return $"{{{nameof(BulkUploadId)}={BulkUploadId}, {nameof(UserId)}={UserId}, {nameof(GroupId)}={GroupId}, {nameof(RequestId)}={RequestId}, {nameof(UserId)}={UserId}}}";
        }
    }
}