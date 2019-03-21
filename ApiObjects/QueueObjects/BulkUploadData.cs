using ApiObjects.Catalog;
using ApiObjects.MediaIndexingObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class BulkUploadData : BaseCeleryData
    {
        #region Consts

        public const string TASK = "distributed_tasks.process_bulk_upload";
        public const string ROUTING_KEY_BULK_UPLOAD = "PROCESS_BULK_UPLOAD\\{0}";

        #endregion

        #region Data Members

        [DataMember]
        //[JsonProperty("bulk_upload_id")]
        [JsonIgnore()]
        private long BulkUploadId { get; set; }

        [DataMember]
        //[JsonProperty("user_id")]
        [JsonIgnore()]
        private long UserId { get; set; }

        #endregion

        public BulkUploadData(int groupId, long bulkUploadId, long userId)
            : base(Guid.NewGuid().ToString(), TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.BulkUploadId = bulkUploadId;
            this.UserId = userId;
            this.args = new List<object>()
            {
                groupId,
                bulkUploadId,
                userId,
                base.RequestId
            };
        }

        public string GetRoutingKey()
        {
            return string.Format(ROUTING_KEY_BULK_UPLOAD, this.GroupId);
        }
    }
}