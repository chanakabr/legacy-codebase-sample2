using ApiObjects.Catalog;
using ApiObjects.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects
{
    public interface IBulkUploadObject
    {
        BulkUploadResult GetNewBulkUploadResult(long bulkUploadId, BulkUploadJobStatus status);
        string DistributedTask { get; }
        string RoutingKey { get; }
        bool Enqueue(int groupId, long userId, long bulkUploadId, BulkUploadJobAction jobAction, int resultIndex);
    }
    
    [Serializable]
    public class BulkUploadObjectData<T>: BaseCeleryData where T : class, IBulkUploadObject
    {
        #region Data Members

        [DataMember]
        [JsonProperty("user_id")]
        private long UserId { get; set; }

        [DataMember]
        [JsonProperty("bulk_upload_id")]
        private long BulkUploadId { get; set; }

        [DataMember]
        [JsonProperty("job_action")]
        public BulkUploadJobAction JobAction { get; set; }

        [DataMember]
        [JsonProperty("result_index")]
        private int ResultIndex { get; set; }

        [DataMember]
        [JsonProperty("object_Data")]
        private T ObjectData { get; set; }

        #endregion

        public BulkUploadObjectData(string task, int groupId, long userId, long bulkUploadId, BulkUploadJobAction jobAction, int resultIndex, T objectData)
            : base(Guid.NewGuid().ToString(), task)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.UserId = userId;
            this.BulkUploadId = bulkUploadId;
            this.JobAction = jobAction;
            this.ResultIndex = resultIndex;
            this.ObjectData = objectData;
            this.args = new List<object>()
            {
                groupId,
                userId,
                bulkUploadId,
                jobAction,
                resultIndex,
                objectData,
                base.RequestId
            };
        }
    }
}