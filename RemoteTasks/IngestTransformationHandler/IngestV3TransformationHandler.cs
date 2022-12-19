using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using AdapterClients.IngestTransformation;
using ApiLogic;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.EventBus;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Core.Catalog;
using EventBus.Abstraction;
using Phx.Lib.Log;
using Core.Catalog.CatalogManagement;
using Core.Catalog.CatalogManagement.Services;
using Core.Profiles;
using EventBus.RabbitMQ;
using IngestHandler.Common;
using IngestHandler.Common.Managers;
using Synchronizer;
using Tvinci.Core.DAL;
using ApiLogic.IndexManager.Helpers;
using Confluent.Kafka;
using EventBus.Kafka;
using FeatureFlag;
using Ingesthandler.common.Generated.Api.Events.ChannelIngestStaged;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Locking;
using IngestHandler.Common.Repositories;
using MongoDB.Driver;
using OTT.Lib.Kafka;
using OTT.Lib.MongoDB;
using EpgBL;

namespace IngestTransformationHandler
{
    public class IngestV3TransformationHandler
    {
        private readonly IIngestStagingRepository _ingestStagingRepository;
        private readonly IKafkaProducerFactory _kafkaProducerFactory;
        private readonly IKafkaContextProvider _kafkaContextProvider;
        private readonly IXmlTvDeserializer _xmlTvDeserializer;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        

        private BulkUpload _bulkUpload;
        private BulkUploadIngestJobData _jobData;
        private BulkUploadEpgAssetData _objectData;
        private string _logPrefix;


        private readonly IKafkaProducer<string, ChannelIngestStaged> _kafkaChannelStagedProducer;

        public IngestV3TransformationHandler(
            IIngestStagingRepository ingestStagingRepository,
            IKafkaProducerFactory kafkaProducerFactory,
            IKafkaContextProvider kafkaContextProvider,
            IXmlTvDeserializer xmlTvDeserializer,
            IEpgIngestMessaging epgIngestMessaging)
        {
            _ingestStagingRepository = ingestStagingRepository;
            _kafkaProducerFactory = kafkaProducerFactory;
            _kafkaContextProvider = kafkaContextProvider;
            _xmlTvDeserializer = xmlTvDeserializer;
            _epgIngestMessaging = epgIngestMessaging;
            _kafkaChannelStagedProducer = _kafkaProducerFactory.Get<string, ChannelIngestStaged>(_kafkaContextProvider, Partitioner.Consistent);
        }

        public async Task Handle(BulkUploadTransformationEvent serviceEvent)
        {
            try
            {
                _logger.Debug($"Starting ingest transformation handler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                InitHandlerProperties(serviceEvent);
                UpdateBulkUpload(BulkUploadJobStatus.Parsing);

                var validationResult = ValidateBulkUpload();
                if (validationResult == Status.Error)
                {
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    // TODO: check if it shouldn't be fatal with Valadimir...
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                }

                var result = _xmlTvDeserializer.DeserializeXmlTv(_bulkUpload.GroupId,_bulkUpload.Id,_jobData.IngestProfileId, _bulkUpload.FileURL);
                _bulkUpload.Results = result.Objects;
                _bulkUpload.NumOfObjects = result.Objects.Count();
                if (!result.IsOkStatusCode())
                {
                    // failed to parse, update status to failed, and compete
                    _bulkUpload.AddError(result.Status);
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    SendIngestPartCompleted();
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                    return;
                }

                
                if (_bulkUpload.NumOfObjects == 0)
                {
                    _logger.Warn($"received an empty deserialized result from ingest, groupId:[{_bulkUpload.GroupId}], bulkUploadId:[{_bulkUpload.Id}]");
                    UpdateBulkUpload(BulkUploadJobStatus.Success);
                    return;
                }
                
                var targetDates = BulkUploadMethods.CalculateIngestDates(_bulkUpload.Results);
                _jobData.DatesOfProgramsToIngest = targetDates.Distinct().ToArray();
                _logger.Info($"Transformation successful, setting results in couchbase, , groupId:[{_bulkUpload.GroupId}], bulkUploadId:[{_bulkUpload.Id}]");
                UpdateBulkUpload(BulkUploadJobStatus.Processing);

                var programs = _bulkUpload.Results.Select(r => r.Object).Cast<EpgProgramBulkUploadObject>().ToList();
                SetProgramsWithEpgIds(programs);
                await _ingestStagingRepository.InsertProgramsToStagingCollection(_bulkUpload.GroupId, programs);

                var channelIds = programs.Select(p => p.ChannelId).Distinct();
                foreach (var channelId in channelIds)
                {
                    var msg = new ChannelIngestStaged
                    {
                        PartnerId = _bulkUpload.GroupId,
                        BulkUploadId = _bulkUpload.Id,
                        LinearChannelId = channelId
                    };
                    await _kafkaChannelStagedProducer.ProduceAsync(ChannelIngestStaged.GetTopic(), msg.GetPartitioningKey(), msg);
                }

                UpdateBulkUpload(BulkUploadJobStatus.Processed);
            }
            catch (Exception ex)
            {
                _logger.Error($"An Exception occurred in BulkUploadTransformationHandler BulkUploadId:[{_bulkUpload.Id}].", ex);
                try
                {
                    _logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{_bulkUpload.Id}]", ex);
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _bulkUpload.AddError(eResponseStatus.Error, $"An unexpected error occurred during transformation handler, {ex.Message}");
                    _logger.Error($"Trying to set fatal status on BulkUploadId:[{serviceEvent.BulkUploadId}]", ex);
                    var result = UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus.Fatal);
                    _logger.Error($"An Exception occurred in transformation handler, BulkUploadId:[{_bulkUpload.Id}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    _logger.Error($"An Exception occurred when trying to set FATAL status on bulkUpload. requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", innerEx);
                    throw;
                }

                throw;
            }
        }


        private void SetProgramsWithEpgIds(List<EpgProgramBulkUploadObject> progs)
        {
            _logger.Debug($"SetProgramsWithEpgIds {_logPrefix} > {progs}");
            var epgBl = new TvinciEpgBL(_bulkUpload.GroupId);
            if (progs.Any())
            {
                var newIds = epgBl.GetNewEpgIds(progs.Count).ToList();
                for (int i = 0; i < progs.Count; i++)
                {
                    var idToSet = newIds[i];
                    progs.ElementAt(i).EpgId = idToSet;
                }
            }
        }

        private void InitHandlerProperties(BulkUploadTransformationEvent serviceEvent)
        {
            _bulkUpload = BulkUploadMethods.GetBulkUploadData(serviceEvent.GroupId, serviceEvent.BulkUploadId);
            _bulkUpload.UpdaterId = serviceEvent.UserId;

            _jobData = _bulkUpload.JobData as BulkUploadIngestJobData;
            if (_jobData == null) { throw new ArgumentException("bulUploadObject.JobData expected to be BulkUploadIngestJobData"); }

            _objectData = _bulkUpload.ObjectData as BulkUploadEpgAssetData;
            if (_objectData == null) { throw new ArgumentException("bulUploadObject.ObjectData expected to be BulkUploadEpgAssetData"); }
            _logPrefix = $"[{(_bulkUpload.GroupId)}-{_bulkUpload}] >";
        }

        private Status ValidateBulkUpload()
        {
            if (_bulkUpload.JobData != null && _bulkUpload.ObjectData != null)
            {
                return Status.Ok;
            }

            // else update the bulk upload object that an error occurred
            _logger.Error($"ProcessBulkUpload cannot Deserialize file because JobData or ObjectData are null for groupId:{_bulkUpload.GroupId}, bulkUploadId:{_bulkUpload.Id}.");
            _bulkUpload.AddError(eResponseStatus.Error, $"Error validate bulk upload. groupId: {_bulkUpload.GroupId}, bulkUploadId: {_bulkUpload.Id}");
            UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus.Fatal);
            return Status.Error;
        }

        private IngestProfile GetIngestProfile(int groupId, int? ingestProfileId)
        {
            var ingestProfile = IngestProfileManager.GetIngestProfileById(groupId, ingestProfileId)?.Object;

            if (ingestProfile == null)
            {
                var message = $"Received bulk upload ingest event with invalid ingest profile.";
                _logger.Error(message);
                throw new Exception(message);
            }

            return ingestProfile;
        }

        private void SendIngestPartCompleted()
        {
            if (_bulkUpload.Results == null || !_bulkUpload.Results.Any()) { return; }

            var parametersList = _bulkUpload
                .Results
                .Cast<BulkUploadProgramAssetResult>()
                .GroupBy(x => (x.LiveAssetId, x.StartDate.Date))
                .Select(results => new EpgIngestPartCompletedParameters
                {
                    BulkUploadId = _bulkUpload.Id,
                    GroupId = _bulkUpload.GroupId,
                    HasMoreEpgToIngest = true,
                    UserId = _bulkUpload.UpdaterId,
                    Results = results
                })
                .ToList();

            if (parametersList.Any()) { parametersList.Last().HasMoreEpgToIngest = false; }

            _epgIngestMessaging.EpgIngestPartCompleted(parametersList);
        }
        
        private void UpdateBulkUpload(BulkUploadJobStatus newStatus)
        {
            var result = BulkUploadManager.UpdateBulkUpload(_bulkUpload, newStatus);
            if (result.IsOkStatusCode() && BulkUpload.IsProcessCompletedByStatus(newStatus))
            {
                SendIngestCompleted(newStatus);
            }
        }

        private GenericResponse<BulkUpload> UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus newStatus)
        {
            var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, newStatus);
            if (result.IsOkStatusCode() && BulkUpload.IsProcessCompletedByStatus(newStatus))
            {
                SendIngestCompleted(newStatus);
            }

            return result;
        }

        private void SendIngestCompleted(BulkUploadJobStatus newStatus)
        {
            var updateDate = DateTime.UtcNow; // TODO looks like _bulUpload.UpdateDate is not updated in CB
            var parameters = new EpgIngestCompletedParameters
            {
                GroupId = _bulkUpload.GroupId,
                BulkUploadId = _bulkUpload.Id,
                Status = newStatus,
                Errors = _bulkUpload.Errors,
                CompletedDate = updateDate,
                UserId = _bulkUpload.UpdaterId,
                Results = _bulkUpload.Results
            };

            _epgIngestMessaging.EpgIngestCompleted(parameters);
        }
    }
}