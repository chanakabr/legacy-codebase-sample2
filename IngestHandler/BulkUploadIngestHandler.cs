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

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Consts

        public static readonly string DEFAULT_INDEX_TYPE = "epg";

        #endregion

        #region Data Members

        ElasticSearchApi elasticSearchClient = null;

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
                DateTime minStartDate = programsToIngest.Min(program => program.StartDate);
                DateTime maxEndDate = programsToIngest.Max(program => program.EndDate);

                var currentPrograms = GetProgramsByDate(groupId, minStartDate, maxEndDate);

                CalculateCRUDOperations(groupId, currentPrograms, programsToIngest);
            }


            throw new NotImplementedException();
        }

        private void CalculateCRUDOperations(int groupId, List<EpgCB> currentPrograms, List<EpgCB> programsToIngest)
        {
            throw new NotImplementedException();
        }

        private List<EpgCB> GetProgramsByDate(int groupId, DateTime minStartDate, DateTime maxEndDate)
        {
            List<EpgCB> result = new List<EpgCB>();
            string index = GetProgramIndexAlias(groupId);
            string type = DEFAULT_INDEX_TYPE;

            FilteredQuery query = new FilteredQuery(true);

            string searchQuery = query.ToString();
            var searchResult = elasticSearchClient.Search(index, type, ref searchQuery);

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
