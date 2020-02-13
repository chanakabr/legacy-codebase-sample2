using ApiObjects.BulkUpload;
using Core.Catalog.CatalogManagement;
using DalCB;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using System.Linq;
using ConfigurationManager;
using ApiObjects;
using Core.Catalog;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestHandler.Common
{
    public class BulkUploadMethods
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly int DEFAULT_CATCHUP_DAYS = 7;
        internal static readonly int EXPIRY_DATE_DELTA = (ApplicationConfiguration.EPGDocumentExpiry.IntValue > 0) ? ApplicationConfiguration.EPGDocumentExpiry.IntValue : 7;

        public static BulkUpload GetBulkUploadData(int groupId, long bulkUploadId)
        {
            var bulkUploadData = BulkUploadManager.GetBulkUpload(groupId, bulkUploadId);

            if (bulkUploadData?.Object == null)
            {
                string message = string.Empty;

                if (bulkUploadData != null && bulkUploadData.Status != null)
                {
                    message = bulkUploadData.Status.Message;
                }

                throw new Exception($"Received invalid bulk upload. group id = {groupId} id = {bulkUploadId} message = {message}");
            }

            return bulkUploadData.Object;
        }

        /// <summary>
        /// create new documents for ALL epgs - generate document key {epg_id}_{language}_{bulk_upload_id}
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="calculatedPrograms"></param>
        /// <param name="bulkUploadId"></param>
        public static async Task UpdateCouchbase(List<EpgProgramBulkUploadObject> calculatedPrograms, int groupId)
        {
            var dal = new EpgDal_Couchbase(groupId);
            // tcm configurable?
            int retryCount = 3;
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (ex, time, attempt, ctx) =>
                {
                    // TODO: improve logging
                    _Logger.Warn("Error while trying to upsert EPG to couchbase", ex);
                    _Logger.Warn($"couchbase upsert retry attempt:[{attempt}/{retryCount}]");
                }
            );

            var insertResult = false;
            await policy.ExecuteAsync(async () =>
            {
                var epgCbObjectToInsert = calculatedPrograms.SelectMany(p => p.EpgCbObjects).ToList();
                SetSearchEndDate(epgCbObjectToInsert, groupId);
                insertResult = await dal.InsertPrograms(epgCbObjectToInsert, EXPIRY_DATE_DELTA);

            });

            if (!insertResult)
            {
                _Logger.Error($"Failed inserting program. group:[{groupId}] bulkUploadId:[{groupId}]");
            }
        }
        
        private static void SetSearchEndDate(List<EpgCB> lEpg, int groupID)
        {
            try
            {
                var days = ApplicationConfiguration.CatalogLogicConfiguration.CurrentRequestDaysOffset.IntValue;
                days = days == 0 ? DEFAULT_CATCHUP_DAYS : days;

                List<string> epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList<string>();
                var linearChannelSettings = BulkUploadIngestJobData.GetLinearChannelSettings(groupID, epgChannelIds);

                Parallel.ForEach(lEpg.Cast<EpgCB>(), currentElement =>
                {
                    var channel = linearChannelSettings.FirstOrDefault(c => c.ChannelID.Equals(currentElement.ChannelID));
                    if (channel == null)
                    {
                        currentElement.SearchEndDate = currentElement.EndDate.AddDays(days);
                    }
                    else if (channel.EnableCatchUp)
                    {
                        currentElement.SearchEndDate =
                            currentElement.EndDate.AddMinutes(channel.CatchUpBuffer);
                    }
                    else
                    {
                        currentElement.SearchEndDate = currentElement.EndDate;
                    }
                });
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error EPG ingest threw an exception. (in GetSearchEndDate). Exception={ex.Message};Stack={ex.StackTrace}", ex);
                throw ex;
            }
        }

        public void AcquireMultiLock(IEnumerable<DateTime> dates)
        {

        }
    }
}
