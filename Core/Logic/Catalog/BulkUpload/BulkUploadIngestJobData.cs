using System;
using System.Reflection;
using System.Xml.Serialization;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using KLogMonitor;
using Newtonsoft.Json;

namespace Core.Catalog
{
    /// <summary>
    /// Instructions for ingest of custom data file
    /// </summary>
    [Serializable]
    [JsonObject(ItemTypeNameHandling = TypeNameHandling.All)]
    public class BulkUploadIngestJobData : BulkUploadJobData
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string XML_TV_DATE_FORMAT = "yyyyMMddHHmmss";
        private const string LOCK_KEY_DATE_FORMAT = "yyyyMMdd";
        private static readonly XmlSerializer _XmltTVserilizer = new XmlSerializer(typeof(EpgChannels));
        public int? IngestProfileId { get; set; }

        public string[] LockKeys;

        public DateTime[] DatesOfProgramsToIngest;

        public override GenericListResponse<BulkUploadResult> Deserialize(int groupId, long bulkUploadId, string fileUrl, BulkUploadObjectData objectData)
        {
            throw new NotImplementedException("Ingest bulk upload deserialization is handled in TransformationHandler and this method should not be called");
        }
    }
}