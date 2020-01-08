using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.BulkUpload
{
    public enum BulkUploadJobStatus
    {
        Pending = 1,
        Uploaded = 2,
        Queued = 3,
        Parsing = 4,
        Processing = 5,
        Processed = 6,
        Success = 7,
        Partial = 8,
        Failed = 9,
        Fatal = 10
    }

    public enum BulkUploadJobAction
    {
        Upsert = 1,
        Delete = 2
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUpload : CoreObject
    {
        public const string NOTIFY_EVENT_NAME = "KalturaBulkUpload";

        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [JsonProperty("FileURL")]
        public string FileURL { get; set; }

        [JsonProperty("FileName")]
        public string FileName { get; set; }

        [JsonProperty("BulkObjectType")]
        public string BulkObjectType { get; set; }

        [JsonProperty("Status")]
        public BulkUploadJobStatus Status { get; set; }

        [JsonProperty("Action")]
        public BulkUploadJobAction Action { get; set; }

        [JsonProperty("NumOfObjects")]
        public int? NumOfObjects { get; set; }

        [JsonProperty("CreateDate")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("UpdateDate")]
        public DateTime UpdateDate { get; set; }

        [JsonProperty("UpdaterId")]
        public long UpdaterId { get; set; }

        [JsonProperty(PropertyName = "Results",
                     TypeNameHandling = TypeNameHandling.Auto,
                     ItemTypeNameHandling = TypeNameHandling.Auto,
                     ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<BulkUploadResult> Results { get; set; }



        /// <summary>
        /// This list of results that are additional to the source input
        /// i.e. side affects of epg ingest may cause existing programs to update.
        /// When they overlap each other the exsistin program migth have to be cut in time to fit the new one
        /// </summary>
        [JsonProperty(PropertyName = "AffectedObjects",
                     TypeNameHandling = TypeNameHandling.Auto,
                     ItemTypeNameHandling = TypeNameHandling.Auto,
                     ItemReferenceLoopHandling = ReferenceLoopHandling.Serialize)]
        public List<IAffectedObject> AffectedObjects { get; set; }

        [JsonProperty("JobData")]
        public BulkUploadJobData JobData { get; set; }

        [JsonProperty("ObjectData")]
        public BulkUploadObjectData ObjectData { get; set; }

        [JsonProperty("Errors")]
        // This is an array and not a list becasue it curntlly serlized by .net core and deserlized with .net45
        // any generic collection will cause a deserlization error
        public Status[] Errors { get; set; }

        public void AddError(Status errorStatus)
        {
            if (errorStatus != null)
            {
                if (Errors == null)
                {
                    Errors = new[] { errorStatus };
                }
                else
                {
                    Errors = Errors.Concat(new[] { errorStatus }).ToArray();
                }
            }
        }

        public void AddError(eResponseStatus errorCode, string msg = "")
        {
            var errorStatus = new Status((int)errorCode, msg);

            AddError(errorStatus);
        }

        public bool IsProcessCompleted => Status == BulkUploadJobStatus.Success || Status == BulkUploadJobStatus.Failed || Status == BulkUploadJobStatus.Partial;

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }
}