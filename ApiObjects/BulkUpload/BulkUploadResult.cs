using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ApiObjects.BulkUpload
{
    public enum BulkUploadResultStatus
    {
        Error = 1,
        Ok = 2,
        InProgress = 3
    }

    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public abstract class BulkUploadResult
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        // can be assetId, userId etc
        [JsonProperty("ObjectId")]
        public long? ObjectId { get; set; }

        [JsonProperty("Index")]
        public int Index { get; set; }

        [JsonProperty("BulkUploadId")]
        public long BulkUploadId { get; set; }

        [JsonProperty("Status")]
        public BulkUploadResultStatus Status { get; set; }

        [JsonProperty("Errors")]
        // This is an array and not a list becasue it curntlly serlized by .net core and deserlized with .net45
        // any generic collection will cause a deserlization error
        public Status[] Errors { get; private set; }

        [JsonProperty("Warnings")]
        // This is an array and not a list becasue it curntlly serlized by .net core and deserlized with .net45
        // any generic collection will cause a deserlization error
        public Status[] Warnings { get; set; }

        [JsonIgnore()]
        public IBulkUploadObject Object { get; set; }

        public BulkUploadResult()
        {
            Index = -1;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("BulkUploadId:{0}, Index:{1}, Status:{2}", BulkUploadId, Index, Status);

            if (ObjectId.HasValue)
            {
                sb.AppendFormat(", ObjectId:{0}", ObjectId);
            }

            if (Errors != null && Errors.Length > 0)
            {
                for (int i = 0; i < Errors.Length; i++)
                {
                    sb.AppendFormat(", Error {0}:{1}", i + 1, Errors[i].ToString());
                }
            }

            if (Warnings != null && Warnings.Length > 0)
            {
                for (int i = 0; i < Warnings.Length; i++)
                {
                    sb.AppendFormat(", Warning {0}:{1}", i + 1, Warnings[i].ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Set the status to Error and update error code and message
        /// </summary>
        /// <param name="errorStatus"></param>
        public void AddError(Status errorStatus)
        {
            this.Status = BulkUploadResultStatus.Error;

            if (errorStatus != null)
            {
                _Logger.Error($"Adding Error to resultIndex:[{Index}], msg:[{errorStatus.Message}]");
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

        public void AddWarning(int warnningCode, string msg = "")
        {
            var warnningStatus = new Status(warnningCode, msg);

            this.Status = BulkUploadResultStatus.Error;

            if (warnningStatus != null)
            {
                _Logger.Error($"Adding Error to resultIndex:[{Index}], msg:[{warnningStatus.Message}]");
                if (Warnings == null)
                {
                    Warnings = new[] { warnningStatus };
                }
                else
                {
                    Warnings = Warnings.Concat(new[] { warnningStatus }).ToArray();
                }
            }
        }
    }
}