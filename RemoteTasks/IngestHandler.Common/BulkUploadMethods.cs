using ApiObjects.BulkUpload;
using Core.Catalog.CatalogManagement;
using DalCB;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Polly;
using System.Linq;
using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using Phx.Lib.Appconfig;
using ApiObjects;
using ApiObjects.Catalog;
using Core.Catalog.Cache;
using Core.GroupManagers;
using ElasticSearch.Utilities;
using Tvinci.Core.DAL;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestHandler.Common
{
    public class BulkUploadResultsDictionary : Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>>
    {
        public BulkUploadResultsDictionary(Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> source)
            : base(source)
        {
        }
    }

    public static class BulkUploadMethods
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly int DEFAULT_CATCHUP_DAYS = 7;
        private const string LOCK_KEY_DATE_FORMAT = "yyyyMMdd";

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
        public static async Task UpdateCouchbase(CRUDOperations<EpgProgramBulkUploadObject> crudOperations, int groupId)
        {
            var programsToAdd = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.AffectedItems)
                .ToList();
            
            var dal = new EpgDal_Couchbase(groupId);
            var retryCount = 3;
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (ex, time, attempt, ctx) =>
                    {
                        _Logger.Warn("Error while trying to upsert EPG to couchbase", ex);
                        _Logger.Warn($"couchbase upsert retry attempt:[{attempt}/{retryCount}]");
                    }
                );

            var insertResult = false;
            // TODO: The retry policy isn't working actually, need to rewrite dal.InsertPrograms() (won't throw exception whatever) and CouchbaseManager.MultiSet()
            await policy.ExecuteAsync(async () =>
            {
                var epgCbObjectToInsert = programsToAdd.SelectMany(p => p.EpgCbObjects).ToList();
                SetSearchEndDate(epgCbObjectToInsert, groupId);
                insertResult = await dal.InsertPrograms(epgCbObjectToInsert, cb => TtlService.Instance.GetEpgCouchbaseTtlSeconds(cb));
            });

            if (!insertResult)
            {
                _Logger.Error($"Failed inserting program. group:[{groupId}] bulkUploadId:[{groupId}]");
            }
        }

        public static IDictionary<string, LanguageObj> GetGroupLanguages(int groupId, out LanguageObj defaultLanguage)
        {
            var languages = GroupLanguageManager.GetGroupLanguages(groupId);
            defaultLanguage = languages.FirstOrDefault(l => l.IsDefault);
            if (defaultLanguage == null) { throw new Exception($"No main language defined for group:[{groupId}], ingest failed"); }

            return languages.ToDictionary(l => l.Code);
        }
        
        public static IEnumerable<DateTime> CalculateIngestDates(IEnumerable<BulkUploadResult> epgBulkUploadResults)
        {
            var allPrograms = epgBulkUploadResults.Where(p => p.Object != null).Select(p => p.Object as EpgProgramBulkUploadObject).ToList();
            var allProgramDates = allPrograms.Select(p => p.StartDate.Date).Distinct().ToList();

            allProgramDates.Add(allPrograms.Max(x => x.StartDate.Date).AddDays(1));
            allProgramDates.Add(allPrograms.Min(x => x.StartDate.Date).AddDays(-1));

            return allProgramDates;
        }


        /// <summary>
        /// create results dictionary to allow us to update any result with errors and warnings during CRUD calculation
        /// dictionary channelId -> epgExternalId -> programObject.
        /// it helps to add errors and warnings without having to loop all results of bulk upload every time
        /// </summary>
        /// <param name="bulkUpload">bulk upload object from which to extract results</param>
        /// <param name="epgObjectToFilterFor">optional filter of epgObject to filter the dictionary for</param>
        /// <returns></returns>
        public static BulkUploadResultsDictionary ConstructResultsDictionary(this BulkUpload bulkUpload, List<EpgProgramBulkUploadObject> epgObjectToFilterFor = null)
        {
            var resultsByChannelAndProgExtId = bulkUpload.Results.Cast<BulkUploadProgramAssetResult>().GroupBy(r => r.ChannelId);
            var resultsDictionary = resultsByChannelAndProgExtId.ToDictionary(r => r.Key, r => r.ToDictionary(p => p.ProgramExternalId));
            if (epgObjectToFilterFor == null) { return new BulkUploadResultsDictionary(resultsDictionary); }

            // if there is a filter sent to use we will fiter the results dictionary
            var filteredDictionary = new Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>>();
            foreach (var prog in epgObjectToFilterFor)
            {
                if (resultsDictionary.TryGetValue(prog.ChannelId, out var progsOfChannel))
                {
                    if (progsOfChannel.TryGetValue(prog.EpgExternalId, out var proResultItem))
                    {
                        if (!filteredDictionary.ContainsKey(prog.ChannelId))
                        {
                            filteredDictionary[prog.ChannelId] = new Dictionary<string, BulkUploadProgramAssetResult>();
                        }
                        filteredDictionary[prog.ChannelId][prog.EpgExternalId] = proResultItem;
                    }
                }
            }

            return new BulkUploadResultsDictionary(filteredDictionary);
        }

        private static void SetSearchEndDate(List<EpgCB> lEpg, int groupID)
        {
            try
            {
                var defaultSearchDaysDelta = ApplicationConfiguration.Current.CatalogLogicConfiguration.CurrentRequestDaysOffset.Value;
                defaultSearchDaysDelta = defaultSearchDaysDelta == 0 ? DEFAULT_CATCHUP_DAYS : defaultSearchDaysDelta;

                var epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList();
                var linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds).Values;

                Parallel.ForEach(lEpg, currentElement =>
                {
                    var channel = linearChannelSettings.FirstOrDefault(c => c.ChannelID.Equals(currentElement.ChannelID.ToString()));
                    if (channel == null)
                    {
                        currentElement.SearchEndDate = currentElement.EndDate.AddDays(defaultSearchDaysDelta);
                    }
                    else if (channel.EnableCatchUp)
                    {
                        currentElement.SearchEndDate = currentElement.EndDate.AddMinutes(channel.CatchUpBuffer);
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
                throw;
            }
        }

        public static string GetIngestLockKey(string indexName)
        {
            return $"Ingest_V2_Lock_{indexName}";
        }
    }
}