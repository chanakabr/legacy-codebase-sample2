using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Ingesthandler.common.Generated.Api.Events.UpdateBulkUpload;
using Microsoft.Extensions.Logging;
using MoreLinq;
using Newtonsoft.Json;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using IngestHandler.Common.Repositories;
using NeoSmart.StreamCompare;
using TVinciShared;

namespace IngestHandler
{
    internal class BulkUploadUpdater : IKafkaMessageHandler<UpdateBulkUpload>
    {
        private readonly IIngestFinalizer _ingestFinalizer;
        private readonly IBulkUploadRepository _bulkUploadRepository;
        private readonly ILogger<BulkUploadUpdater> _logger;
        private readonly StreamCompare _streamCompare;

        public BulkUploadUpdater(IIngestFinalizer ingestFinalizer, IBulkUploadRepository bulkUploadRepository,  ILogger<BulkUploadUpdater> logger)
        {
            _ingestFinalizer = ingestFinalizer;
            _bulkUploadRepository = bulkUploadRepository;
            _logger = logger;
            _streamCompare = new StreamCompare();
        }

        /// <summary>
        /// This consumer using partitioning per bulkUploadId and partnerId, so we will never update same bulk upload concurrently.
        /// The above allows us to get the current object form CB make all required updates in-memory (accumlate crud op, update results etc...)
        /// and then save it as is into CB.
        /// </summary>
        public async Task<HandleResult> Handle(ConsumeResult<string, UpdateBulkUpload> consumeResult)
        {
            try
            {
                var msg = consumeResult.Result.Message.Value;
                // ReSharper disable once PossibleInvalidOperationException - will never be null, nullable due to generator
                var partnerId = consumeResult.Result.Message.Value.PartnerId.Value;
                // ReSharper disable once PossibleInvalidOperationException - will never be null, nullable due to generator
                var bulkUploadId = consumeResult.Result.Message.Value.BulkUploadId.Value;
                
                var results = await _bulkUploadRepository.GetBulkUploadResults(partnerId, bulkUploadId);
                var crudOps = await _bulkUploadRepository.GetCrudOperations(partnerId, bulkUploadId);
                var errors = await _bulkUploadRepository.GetErrors(partnerId, bulkUploadId);
                
                // Get bulk upload object
                var bulkUploadResp = BulkUploadManager.GetBulkUpload((int)partnerId, bulkUploadId);
                if (!bulkUploadResp.Status.IsOkStatusCode())
                {
                    _logger.LogError($"error while trying to get bulkUpload:[{bulkUploadId}], {bulkUploadResp.Status.Code} - {bulkUploadResp.Status.Message}");
                    return new HandleResult();
                }

                var bulkUploadFromDb = bulkUploadResp.Object;
                var originalStatus = bulkUploadFromDb.Status;
                var bulkUploadUpdateCandidate = ObjectCopier.Clone(bulkUploadResp.Object);
                bulkUploadUpdateCandidate.Errors = errors.ToArray();
                bulkUploadUpdateCandidate.AffectedObjects = crudOps.AffectedItems.Cast<IAffectedObject>().ToList();
                bulkUploadUpdateCandidate.AddedObjects = crudOps.ItemsToAdd.Cast<IAffectedObject>().ToList();
                bulkUploadUpdateCandidate.UpdatedObjects = crudOps.ItemsToUpdate.Cast<IAffectedObject>().ToList();
                bulkUploadUpdateCandidate.DeletedObjects = crudOps.ItemsToDelete.Cast<IAffectedObject>().ToList();
                results.ForEach(r => bulkUploadUpdateCandidate.Results[r.Index] = r);

                
                // This is here to optimize the writes to CB. because we have multiple consumers per channel running concurrently
                // its probable that many update requests will come in while most of the data was already written to monog
                // this will cause multiple writes of the same object to CB.
                var areEqual = await CompareBinary(bulkUploadFromDb, bulkUploadUpdateCandidate);
                if (areEqual)
                {
                    _logger.LogInformation($"Update candidate is identical to object from db, bulkUploadId:[{bulkUploadId}], skipping write to CB");
                    return new HandleResult();
                }
                
                
                var statusToSet = BulkUpload.GetBulkStatusByResultsStatus(bulkUploadUpdateCandidate);
                statusToSet = (msg.SetStatusToFatal == true) ? BulkUploadJobStatus.Fatal : statusToSet;
                
                _logger.LogInformation($"updating bulk uploadObject id:[{bulkUploadUpdateCandidate.Id}], statusToSet:[{statusToSet}], bulkUploadObject.Status:[{bulkUploadUpdateCandidate.Status}] originalStatus:[{originalStatus}], shouldSetToFatal:[{msg.SetStatusToFatal}]");
                var updatedBulkUploadResponse = BulkUploadManager.UpdateBulkUpload(bulkUploadUpdateCandidate, statusToSet);
                if (!updatedBulkUploadResponse.IsOkStatusCode())
                {
                    _logger.LogError($"error while trying to update bulk upload object, {updatedBulkUploadResponse.Status}");
                }
                
                await _ingestFinalizer.FinalizeEpgV3Ingest((int)partnerId, updatedBulkUploadResponse.Object);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"error while to update bulkUpload:[{consumeResult.Result.Message.Value.BulkUploadId}]");
            }

            return new HandleResult();
        }

        private async Task<bool> CompareBinary(object obj1, object obj2)
        {
            var bf = new BinaryFormatter();
            using (var ms1 = new MemoryStream())
            using (var ms2 = new MemoryStream())
            {
                bf.Serialize(ms1, obj1);
                bf.Serialize(ms2, obj2);
                ms1.Seek(0, SeekOrigin.Begin);
                ms2.Seek(0, SeekOrigin.Begin);
                var areEqual = await _streamCompare.AreEqualAsync(ms1, ms2, true);
                return areEqual;
            }
        }
    }
}
