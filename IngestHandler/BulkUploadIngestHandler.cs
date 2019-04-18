using ApiObjects.BulkUpload;
using ApiObjects.EventBus;
using Core.Catalog.CatalogManagement;
using EventBus.Abstraction;
using KLogMonitor;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects;
using System.Collections.Generic;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using DalCB;
using Newtonsoft.Json.Linq;
using Core.Catalog;
using Core.Profiles;
using CouchbaseManager;

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Consts

        public static readonly string DEFAULT_INDEX_TYPE = "epg";
        public static readonly string EPG_SEQUENCE_DOCUMENT = "epg_sequence_document";

        #endregion

        #region Data Members

        private ElasticSearchApi elasticSearchClient = null;
        
        #endregion

        #region Ctor

        public BulkUploadIngestHandler()
        {
            elasticSearchClient = new ElasticSearchApi();
        }

        #endregion
        #region Public Methods

        public Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                log.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                
                ValidateServiceEvent(serviceEvent);
              
            }
            catch (Exception ex)
            {
                log.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                return Task.FromException(ex);
            }

            return Task.CompletedTask;

        }

        #endregion

        #region Private Methods

        // TODO: for sunny
        // Get Ingest Profile by ingest Profile ID
        // according to defaultAutoFillPolicy if 2 and have holes reject input
        // 
        private void ValidateServiceEvent(BulkUploadIngestEvent serviceEvent)
        {
            if (serviceEvent.ProgramsToIngest == null)
            {
                throw new Exception($"Received bulk upload ingest event with null programs to insert. group id ={serviceEvent.GroupId} id = {serviceEvent.BulkUploadId}");
            }

            var bulkUploadData = BulkUploadManager.GetBulkUpload(serviceEvent.GroupId, serviceEvent.BulkUploadId);

            if (bulkUploadData == null || bulkUploadData.Object == null)
            {
                string message = string.Empty;

                if (bulkUploadData != null && bulkUploadData.Status != null)
                {
                    message = bulkUploadData.Status.Message;
                }

                throw new Exception($"Received invalid bulk upload. group id ={serviceEvent.GroupId} id = {serviceEvent.BulkUploadId} message = {message}");
            }
            int groupId = serviceEvent.GroupId;
            var programsToIngest = serviceEvent.ProgramsToIngest.ToList();

            if (programsToIngest.Count() == 0)
            {
                log.Warn($"Received bulk upload ingest event with 0 programs to insert. group id ={serviceEvent.GroupId} id = {serviceEvent.BulkUploadId}");
            }
            else
            {
                var programAssetResultsDictionary = bulkUploadData.Object.Results.Cast<BulkUploadProgramAssetResult>().ToList().ToDictionary(program => program.ProgramExternalId);

                foreach (var programToIngest in programsToIngest)
                {
                    programAssetResultsDictionary[programToIngest.EpgIdentifier].Object = programToIngest;
                }

                DateTime minStartDate = programsToIngest.Min(program => program.StartDate);
                DateTime maxEndDate = programsToIngest.Max(program => program.EndDate);

                var currentPrograms = GetCurrentProgramsByDate(groupId, minStartDate, maxEndDate);

                List<EpgCB> programsToAdd;
                List<EpgCB> programsToUpdate;
                List<EpgCB> programsToDelete;

                CalculateCRUDOperations(groupId, currentPrograms, programsToIngest, out programsToAdd, out programsToUpdate, out programsToDelete);

                List<EpgCB> caluclatedPrograms = CalculateSimulatedFinalStateAfterIngest(programsToAdd, programsToUpdate);

                // duplicate the index of this day
                var dateOfIngest = serviceEvent.DateOfProgramsToIngest;

                var ingestProfileId = (bulkUploadData.Object.JobData as BulkUploadIngestJobData).IngestProfileId;

                var profile = IngestProfileManager.GetIngestProfileById(ingestProfileId)?.Object;


                
                // update bulk upload in the end
                BulkUploadManager.UpdateBulkUpload(bulkUploadData.Object);
            }
        }

        private List<EpgCB> CalculateSimulatedFinalStateAfterIngest(List<EpgCB> programsToAdd, List<EpgCB> programsToUpdate)
        {
            List<EpgCB> result = new List<EpgCB>();

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);

            // set the new programs with new IDs from sequence document in couchbase
            foreach (var program in programsToAdd)
            {
                program.EpgID = couchbaseManager.Increment(EPG_SEQUENCE_DOCUMENT, 1);
            }

            // union lists and order by start date
            result.Union(programsToAdd);
            result.Union(programsToUpdate);
            result = result.OrderBy(program => program.StartDate).ToList();

            return result;
        }

        private void CalculateCRUDOperations(int groupId, List<EpgCB> currentPrograms, List<EpgCB> programsToIngest,
            out List<EpgCB> programsToAdd, out List<EpgCB> programsToUpdate, out List<EpgCB> programsToDelete)
        {
            programsToAdd = new List<EpgCB>();
            programsToUpdate = new List<EpgCB>();
            programsToDelete = new List<EpgCB>();

            var currentProgramsDictionary = currentPrograms.ToDictionary(epg => epg.EpgIdentifier);
            var programsToIngestDictionary = programsToIngest.ToDictionary(epg => epg.EpgIdentifier);

            
            foreach (var programToIngest in programsToIngestDictionary)
            {
                // if a program exists both on newly ingested epgs and in index - it's an update
                if (currentProgramsDictionary.ContainsKey(programToIngest.Key))
                {
                    // update the epg id of the ingested programs with their existing epg id from CB
                    programToIngest.Value.EpgID = currentProgramsDictionary[programToIngest.Key].EpgID;
                    programsToUpdate.Add(programToIngest.Value);
                }
                else
                {
                    // if it exists only on newly ingested epgs and not in index, it's a program to add
                    programsToAdd.Add(programToIngest.Value);
                }
            }

            foreach (var currentProgram in currentProgramsDictionary)
            {
                // if a program exists in index but not in newly ingested programs, it should be deleted
                if (!programsToIngestDictionary.ContainsKey(currentProgram.Key))
                {
                    programsToDelete.Add(currentProgram.Value);
                }
            }
        }

        private List<EpgCB> GetCurrentProgramsByDate(int groupId, DateTime minStartDate, DateTime maxEndDate)
        {
            List<EpgCB> result = new List<EpgCB>();
            string index = GetProgramIndexAlias(groupId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!elasticSearchClient.IndexExists(index))
            {
                return result;
            }

            string type = DEFAULT_INDEX_TYPE;

            FilteredQuery query = new FilteredQuery(true);

            // Program end date > minimum start date
            ESRange minimumRange = new ESRange(false)
            {
                Key = "end_date"
            };
            minimumRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, minStartDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT)));

            // program start date < maximum end date
            ESRange maximumRange = new ESRange(false)
            {
                Key = "start_date"
            };
            maximumRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, maxEndDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT)));

            FilterCompositeType filterCompositeType = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            filterCompositeType.AddChild(minimumRange);
            filterCompositeType.AddChild(maximumRange);

            query.Filter = new QueryFilter()
            {
                FilterSettings = filterCompositeType
            };

            string searchQuery = query.ToString();
            var searchResult = elasticSearchClient.Search(index, type, ref searchQuery);

            List<string> epgIds = new List<string>();

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                JObject json = JObject.Parse(searchResult);

                var hits = (json["hits"] as JArray);

                foreach (var hit in hits)
                {
                    epgIds.Add(hit["epg_id"].ToString());
                }

                result = new EpgDal_Couchbase(groupId).GetProgram(epgIds);
            }

            return result;
        }

        #endregion

        #region Utility methods

        private string GetProgramIndexAlias(int groupId)
        {
            return $"{groupId}_epg_v2";
        }

        #endregion
    }

}
