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
using FeatureFlag;
using IngestHandler.Common.Locking;

namespace IngestTransformationHandler
{
    public class IngestV2TransformationHandler
    {
        private readonly IndexCompactionManager _indexCompactionManager;
        private readonly IEpgCRUDOperationsManager _crudOperationsManager;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private readonly IXmlTvDeserializer _xmlTvDeserializer;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly XmlSerializer _XmlTVSerializer = new XmlSerializer(typeof(EpgChannels));

        private BulkUploadTransformationEvent _eventData;
        private BulkUpload _bulkUpload;
        private BulkUploadIngestJobData _jobData;
        private BulkUploadEpgAssetData _objectData;
        private LanguagesInfo _languagesInfo;
        private LanguageObj _defaultLanguage;

        private IngestProfile _ingestProfile;
        private DistributedLock _locker;
        
        public IngestV2TransformationHandler(
            IndexCompactionManager indexCompactionManager, 
            IEpgCRUDOperationsManager crudOperationsManager, 
            IEpgIngestMessaging epgIngestMessaging, 
            IXmlTvDeserializer xmlTvDeserializer,
            IPhoenixFeatureFlag phoenixFeatureFlag)
        {
            _indexCompactionManager = indexCompactionManager;
            _crudOperationsManager = crudOperationsManager;
            _epgIngestMessaging = epgIngestMessaging;
            _xmlTvDeserializer = xmlTvDeserializer;
            _phoenixFeatureFlag = phoenixFeatureFlag;
        }

        public Task Handle(BulkUploadTransformationEvent serviceEvent)
        {
            try
            {
                _logger.Debug($"Starting ingest transformation handler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                _indexCompactionManager.RunEpgIndexCompactionIfRequired(serviceEvent.GroupId, serviceEvent.BulkUploadId);
                
                InitHandlerProperties(serviceEvent);
                UpdateBulkUpload(BulkUploadJobStatus.Parsing);

                var validationResult = ValidateBulkUpload();
                if (validationResult == Status.Error)
                {
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                    return Task.CompletedTask;
                }

                var result = _xmlTvDeserializer.DeserializeXmlTv(_bulkUpload.GroupId,_bulkUpload.Id,_jobData.IngestProfileId, _bulkUpload.FileURL);
                if (!result.IsOkStatusCode())
                {
                    // failed to parse, update status to failed, and compete
                    _bulkUpload.AddError(result.Status);
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    SendIngestPartCompleted();
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                }

                _bulkUpload.Results = result.Objects.Cast<BulkUploadResult>().ToList();
                _bulkUpload.NumOfObjects = _bulkUpload.Results.Count();
                if (_bulkUpload.NumOfObjects == 0)
                {
                    _logger.Warn($"received an empty deserialized result from ingest, groupId:[{_bulkUpload.GroupId}], bulkUploadId:[{_bulkUpload.Id}]");
                    UpdateBulkUpload(BulkUploadJobStatus.Success);
                    return Task.CompletedTask;
                }

                // use update bulk upload and not update with version check or update results because
                // updateBulkUpload: method just dumps the entire object into CB without any checks - initial creation
                // updateStatusWithVersionCheck: intended to use when you need to update the status only with a lock in multiprocess mode (not the case in transformation handler)
                // updateResults: will update existing result according to new result set
                // todo: arthur: talk to lior about dropping this madness that shir used for VOD bulk uploads, and using one single save ,method, this is crazzzzyyyy.... 
                _logger.Info($"Transformation successful, setting results in couchbase, , groupId:[{_bulkUpload.GroupId}], bulkUploadId:[{_bulkUpload.Id}]");
                UpdateBulkUpload(BulkUploadJobStatus.Processing);

                // start lock before calculating crude so that the schedule will not change while we try to calculate and ingest
                var targetDates = BulkUploadMethods.CalculateIngestDates(_bulkUpload.Results);
                var targetIndices = targetDates.Select(d => NamingHelper.Instance.GetDailyEpgIndexName(_eventData.GroupId, d));
                _jobData.LockKeys = targetIndices.Select(i => BulkUploadMethods.GetIngestLockKey(i)).Distinct().ToArray();
                _jobData.DatesOfProgramsToIngest = targetDates.Distinct().ToArray();
                AcquireLockOnIngestRange();

                var crudOperations = _crudOperationsManager.CalculateCRUDOperations(_bulkUpload, _ingestProfile.DefaultOverlapPolicy, _ingestProfile.DefaultAutoFillPolicy, _languagesInfo);
                if (_bulkUpload.Results.Any(r => r.Errors?.Any() == true))
                {
                    _bulkUpload.AddError(eResponseStatus.Error, "error while trying to calculate required changes to Epg, see results for more information");
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    SendIngestPartCompleted();
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                    Unlock();
                    return Task.CompletedTask;
                }

                // todo: arthur, consider unlocking dates that were locked before CRUD calculation (+-1) and evatually were no crud ops required for

                // objects are saved on the bulk upload object, because
                // they are also used by the ingest finalizer for cache invalidation and update recordings quota
                SetToBulkUpload(crudOperations.AffectedItems, (u, i) => u.AffectedObjects = i);
                SetToBulkUpload(crudOperations.ItemsToUpdate, (u, i) => u.UpdatedObjects = i);
                SetToBulkUpload(crudOperations.ItemsToDelete, (u, i) => u.DeletedObjects = i);
                // new items don't have EpgId on this step, EpgId will appear in BulkUploadIngestHandler.SetResultsWithObjectId.
                // and we'll retrieve EpgId by EpgExternalId in IngestFinalizer
                SetToBulkUpload(crudOperations.ItemsToAdd, (u, i) => u.AddedObjects = i);


                UpdateBulkUpload(BulkUploadJobStatus.Processed);
                EnqueueIngestEvents(crudOperations);
            }
            catch (Exception ex)
            {
                _logger.Error($"An Exception occurred in BulkUploadTransformationHandler requestId:[{_eventData.RequestId}], BulkUploadId:[{_eventData.BulkUploadId}].", ex);
                try
                {
                    _logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{_eventData.BulkUploadId}]", ex);
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _bulkUpload.AddError(eResponseStatus.Error, $"An unexpected error occurred during transformation handler, {ex.Message}");
                    _logger.Error($"Trying to set fatal status on BulkUploadId:[{serviceEvent.BulkUploadId}]", ex);
                    var result = UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus.Fatal);
                    _logger.Error($"An Exception occurred in transformation handler requestId:[{_eventData.RequestId}], BulkUploadId:[{_eventData.BulkUploadId}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    _logger.Error($"An Exception occurred when trying to set FATAL status on bulkUpload. requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", innerEx);
                    throw;
                }
                finally
                {
                    Unlock();
                }

                throw;
            }

            return Task.CompletedTask;
        }

        private void InitHandlerProperties(BulkUploadTransformationEvent serviceEvent)
        {
            _eventData = serviceEvent;
            var lockerMetadata = new Dictionary<string, string>
            {
                { "BulkUploadId", serviceEvent.BulkUploadId.ToString() }
            };
            _locker = new DistributedLock(new LockContext(serviceEvent.GroupId, serviceEvent.UserId), _phoenixFeatureFlag, lockerMetadata);

            _bulkUpload = BulkUploadMethods.GetBulkUploadData(serviceEvent.GroupId, serviceEvent.BulkUploadId);

            _jobData = _bulkUpload.JobData as BulkUploadIngestJobData;
            if (_jobData == null) { throw new ArgumentException("bulUploadObject.JobData expected to be BulkUploadIngestJobData"); }

            _objectData = _bulkUpload.ObjectData as BulkUploadEpgAssetData;
            if (_objectData == null) { throw new ArgumentException("bulUploadObject.ObjectData expected to be BulkUploadEpgAssetData"); }

            _ingestProfile = GetIngestProfile();
            _bulkUpload.UpdaterId = serviceEvent.UserId;
            
            var languages = BulkUploadMethods.GetGroupLanguages(_eventData.GroupId, out _defaultLanguage);
            _languagesInfo = new LanguagesInfo
            {
                Languages = languages,
                DefaultLanguage = _defaultLanguage
            };
        }

        private Status ValidateBulkUpload()
        {
            if (_bulkUpload.JobData != null && _bulkUpload.ObjectData != null)
            {
                return Status.Ok;
            }

            // else update the bulk upload object that an error occured
            _logger.Error($"ProcessBulkUpload cannot Deserialize file because JobData or ObjectData are null for groupId:{_bulkUpload.GroupId}, bulkUploadId:{_bulkUpload.Id}.");
            _bulkUpload.AddError(eResponseStatus.Error, $"Error validate bulk upload. groupId: {_bulkUpload.GroupId}, bulkUploadId: {_bulkUpload.Id}");
            UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus.Fatal);
            return Status.Error;
        }

        private void EnqueueIngestEvents(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var ingestEvents = GenerateIngestEvents(crudOperations);
            if (!ingestEvents.Any())
            {
                _logger.Warn($"EnqueueIngestEvents > bulkUpload:[{_bulkUpload.Id}], crudOperations:[{crudOperations}] resulted in an empty list, this might be due to policy set to CUT Source and the items to ingest were completely overlapped, so they were removed");
                Unlock();
                _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Ok);
                UpdateBulkUpload(BulkUploadJobStatus.Success);
                return;
            }

            // in case the actual crud calculations are less thant the keys we locked initially this means we can unlock few days
            var effectiveLocIndices = ingestEvents.Select(e => e.TargetIndexName).ToList();
            var effectiveLockKeys = effectiveLocIndices.Select(targetIndexName => BulkUploadMethods.GetIngestLockKey(targetIndexName));
            var keysToUnlock = _jobData.LockKeys.Except(effectiveLockKeys);
            if (keysToUnlock.Any())
            {
                _logger.Info($"calculated crud operations did not include several days that were locked, unlocking:[{string.Join(",", keysToUnlock)}]");
                _locker.Unlock(keysToUnlock, LockInitiator.GetBulkUploadLockInitiator(_eventData.BulkUploadId));
                var allCrudOperations = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.ItemsToDelete)
                .Concat(crudOperations.AffectedItems)
                .ToList();
                _jobData.DatesOfProgramsToIngest = allCrudOperations.Select(c=>c.StartDate.Date).Distinct().ToArray();
                _jobData.LockKeys = effectiveLockKeys.Distinct().ToArray();
                UpdateBulkUpload(_bulkUpload.Status);
            }

            var publisher = EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
            publisher.Publish(ingestEvents);
        }

        private List<BulkUploadIngestEvent> GenerateIngestEvents(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var allCrudOperations = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.ItemsToDelete)
                .Concat(crudOperations.AffectedItems)
                .ToList();
            // We do not add remaining as they are here just for the sake of CRUD calculation
            // once we know what should be CRUD-ed, we dont need them anymore because they dont get updated.
            //.Concat(crudOperations.RemainingItems)


            var nh = NamingHelper.Instance;
            var allCrudTargetIndices = allCrudOperations.Select(o => nh.GetDailyEpgIndexName(o.GroupId, o.StartDate.Date)).Distinct().ToList();
            _logger.Debug($"bulkUploadId:[{_bulkUpload.Id}] calculated crud dates:[{string.Join(",", allCrudTargetIndices)}]");

            var ingestEvents = new List<BulkUploadIngestEvent>();
            foreach (var crudIndexName in allCrudTargetIndices)
            {
                var crudOpsOfDay = new CRUDOperations<EpgProgramBulkUploadObject>();
                crudOpsOfDay.ItemsToAdd = crudOperations.ItemsToAdd.Where(i => nh.GetDailyEpgIndexName(i.GroupId, i.StartDate.Date) == crudIndexName).ToList();
                crudOpsOfDay.ItemsToDelete = crudOperations.ItemsToDelete.Where(i => nh.GetDailyEpgIndexName(i.GroupId, i.StartDate.Date) == crudIndexName).ToList();
                crudOpsOfDay.ItemsToUpdate = crudOperations.ItemsToUpdate.Where(i => nh.GetDailyEpgIndexName(i.GroupId, i.StartDate.Date) == crudIndexName).ToList();
                crudOpsOfDay.AffectedItems = crudOperations.AffectedItems.Where(i => nh.GetDailyEpgIndexName(i.GroupId, i.StartDate.Date) == crudIndexName).ToList();

                // we dont add remaining items because of the same comment above ...
                //crudOpsOfDay.RemainingItems.AddRange(crudOperations.RemainingItems.Where(i => i.StartDate.Date == crudDate));
                var ingestEvent = new BulkUploadIngestEvent
                {
                    BulkUploadId = _eventData.BulkUploadId,
                    GroupId = _eventData.GroupId,
                    RequestId = _eventData.RequestId,
                    UserId = _eventData.UserId,
                    TargetIndexName = crudIndexName,
                    CrudOperations = crudOpsOfDay
                };
                ingestEvents.Add(ingestEvent);
            }

            return ingestEvents;
        }

        private void AcquireLockOnIngestRange()
        {
            var epgV2Config = ApplicationConfiguration.Current.EPGIngestV2Configuration;
            var isLocked = _locker.Lock(_jobData.LockKeys,
                epgV2Config.LockNumOfRetries.Value,
                epgV2Config.LockRetryIntervalMS.Value,
                epgV2Config.LockTTLSeconds.Value,
                LockInitiator.GetBulkUploadLockInitiator(_bulkUpload.Id),
                LockInitiator.EpgIngestGlobalLockKeyInitiator);
            if (!isLocked)
            {
                throw new Exception("Failed to acquire lock on ingest dates");
            }
        }

        private void Unlock()
        {
            _locker.Unlock(_jobData.LockKeys, LockInitiator.GetBulkUploadLockInitiator(_bulkUpload.Id));
        }

        private IngestProfile GetIngestProfile()
        {
            var ingestProfile = IngestProfileManager.GetIngestProfileById(_eventData.GroupId, _jobData.IngestProfileId)?.Object;

            if (ingestProfile == null)
            {
                var message = $"Received bulk upload ingest event with invalid ingest profile.";
                _logger.Error(message);
                throw new Exception(message);
            }

            return ingestProfile;
        }

        public void SetToBulkUpload(List<EpgProgramBulkUploadObject> items, Action<BulkUpload, List<IAffectedObject>> setter)
        {
            if (items.Any())
            {
                setter(_bulkUpload, items.Cast<IAffectedObject>().ToList());
            }
        }
        
        private void UpdateBulkUpload(BulkUploadJobStatus newStatus)
        {
            var result = BulkUploadManager.UpdateBulkUpload(_bulkUpload, newStatus);
            if (result.IsOkStatusCode()) TrySendIngestCompleted(newStatus);
        }

        private void SendIngestPartCompleted()
        {
            if (_bulkUpload.Results == null || !_bulkUpload.Results.Any())
            {
                return;
            }

            var parametersList = _bulkUpload
                .Results
                .Where(x => x.Object is EpgProgramBulkUploadObject)
                .GroupBy(x => NamingHelper.Instance.GetDailyEpgIndexName(_bulkUpload.GroupId, ((EpgProgramBulkUploadObject)x.Object).StartDate))
                .Select(x => x.Cast<BulkUploadProgramAssetResult>())
                .Select(MapToEpgIngestPartCompletedParameters)
                .ToList();

            if (parametersList.Count > 0)
            {
                parametersList.Last().HasMoreEpgToIngest = false;
            }

            _epgIngestMessaging.EpgIngestPartCompleted(parametersList);
        }

        private EpgIngestPartCompletedParameters MapToEpgIngestPartCompletedParameters(IEnumerable<BulkUploadProgramAssetResult> results)
            => new EpgIngestPartCompletedParameters
            {
                BulkUploadId = _bulkUpload.Id,
                GroupId = _bulkUpload.GroupId,
                HasMoreEpgToIngest = true,
                UserId = _bulkUpload.UpdaterId,
                Results = results
            };
        
        private GenericResponse<BulkUpload> UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus newStatus)
        {
            var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, newStatus);
            if (result.IsOkStatusCode()) TrySendIngestCompleted(newStatus);

            return result;
        }

        private void TrySendIngestCompleted(BulkUploadJobStatus newStatus)
        {
            if (!BulkUpload.IsProcessCompletedByStatus(newStatus)) return;
            
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
