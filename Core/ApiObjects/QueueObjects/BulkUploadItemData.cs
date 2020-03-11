using ApiObjects.BulkUpload;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects
{
    [Serializable]
    public class BulkUploadItemData<T>: BaseCeleryData where T : class, IBulkUploadObject
    {
        #region Data Members

        [DataMember]
        //[JsonProperty("user_id")]
        [JsonIgnore()]
        private long UserId { get; set; }

        [DataMember]
        //[JsonProperty("bulk_upload_id")]
        [JsonIgnore()]
        private long BulkUploadId { get; set; }

        [DataMember]
        //[JsonProperty("job_action")]
        [JsonIgnore()]
        public BulkUploadJobAction JobAction { get; set; }

        [DataMember]
        //[JsonProperty("result_index")]
        [JsonIgnore()]
        private int ResultIndex { get; set; }

        [DataMember]
        //[JsonProperty("object_Data")]
        [JsonIgnore()]
        private T ObjectData { get; set; }

        #endregion

        public BulkUploadItemData(string task, int groupId, long userId, long bulkUploadId, BulkUploadJobAction jobAction, int resultIndex, T objectData)
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