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
using IngestTransformationHandler.Managers;
using Synchronizer;
using Tvinci.Core.DAL;
using ApiLogic.IndexManager.Helpers;

namespace IngestTransformationHandler
{
    public class BulkUploadTransformationHandler : IServiceEventHandler<BulkUploadTransformationEvent>
    {
        private readonly IndexCompactionManager _indexCompactionManager;
        private readonly IEpgCRUDOperationsManager _crudOperationsManager;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly XmlSerializer _XmlTVSerializer = new XmlSerializer(typeof(EpgChannels));

        private BulkUploadTransformationEvent _eventData;
        private BulkUpload _bulUpload;
        private BulkUploadIngestJobData _jobData;
        private BulkUploadEpgAssetData _objectData;
        private IDictionary<string,LanguageObj> _languages;
        private LanguagesInfo _languagesInfo;
        private LanguageObj _defaultLanguage;

        private IngestProfile _ingestProfile;
        private DistributedLock _locker;

        public BulkUploadTransformationHandler(IndexCompactionManager indexCompactionManager, IEpgCRUDOperationsManager crudOperationsManager, IEpgIngestMessaging epgIngestMessaging)
        {
            _indexCompactionManager = indexCompactionManager;
            _crudOperationsManager = crudOperationsManager;
            _epgIngestMessaging = epgIngestMessaging;
        }

        public Task Handle(BulkUploadTransformationEvent serviceEvent)
        {
            try
            {
                _logger.Debug($"Starting ingest transformation handler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                _indexCompactionManager.RunEpgIndexCompactionIfRequired(serviceEvent.GroupId);
                
                InitHandlerProperties(serviceEvent);
                UpdateBulkUpload(BulkUploadJobStatus.Parsing);

                var validationResult = ValidateBulkUpload();
                if (validationResult == Status.Error)
                {
                    _bulUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                    return Task.CompletedTask;
                }

                var isDeserializeAndSetResultsSuccess = DeserializeAndSetResults();
                if (!isDeserializeAndSetResultsSuccess)
                {
                    // failed to parse, update status to failed, and compete
                    _bulUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    UpdateBulkUpload(BulkUploadJobStatus.Failed);
                    return Task.CompletedTask;
                }

                _bulUpload.NumOfObjects = _bulUpload.Results.Count();
                if (_bulUpload.NumOfObjects == 0)
                {
                    _logger.Warn($"received an empty deserialized result from ingest, groupId:[{_bulUpload.GroupId}], bulkUploadId:[{_bulUpload.Id}]");
                    UpdateBulkUpload(BulkUploadJobStatus.Success);
                    return Task.CompletedTask;
                }

                // use update bulk upload and not update with version check or update results because
                // updateBulkUpload: method just dumps the entire object into CB without any checks - initial creation
                // updateStatusWithVersionCheck: intended to use when you need to update the status only with a lock in multiprocess mode (not the case in transformation handler)
                // updateResults: will update existing result according to new result set
                // todo: arthur: talk to lior about dropping this madness that shir used for VOD bulk uploads, and using one single save ,method, this is crazzzzyyyy.... 
                _logger.Info($"Transformation successful, setting results in couchbase, , groupId:[{_bulUpload.GroupId}], bulkUploadId:[{_bulUpload.Id}]");
                UpdateBulkUpload(BulkUploadJobStatus.Processing);

                // start lock before calculating crude so that the schedule will not change while we try to calculate and ingest
                var targetDates = CalculateIngestDates(_bulUpload.Results);
                var targetIndices = targetDates.Select(d => NamingHelper.Instance.GetDailyEpgIndexName(_eventData.GroupId, d));
                _jobData.LockKeys = targetIndices.Select(i => BulkUploadMethods.GetIngestLockKey(i)).Distinct().ToArray();
                _jobData.DatesOfProgramsToIngest = targetDates.Distinct().ToArray();
                AcquireLockOnIngestRange();

                var crudOperations = _crudOperationsManager.CalculateCRUDOperations(_bulUpload, _ingestProfile.DefaultOverlapPolicy, _ingestProfile.DefaultAutoFillPolicy, _languagesInfo);
                if (_bulUpload.Results.Any(r => r.Errors?.Any() == true))
                {
                    _bulUpload.AddError(eResponseStatus.Error, "error while trying to calculate required changes to Epg, see results for more information");
                    _bulUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
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
                    _bulUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _bulUpload.AddError(eResponseStatus.Error, $"An unexpected error occurred during transformation handler, {ex.Message}");
                    _logger.Error($"Trying to set fatal status on BulkUploadId:[{serviceEvent.BulkUploadId}]", ex);
                    var result = UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus.Fatal);
                    _logger.Error($"An Exception occurred in transformation handler requestId:[{_eventData.RequestId}], BulkUploadId:[{_eventData.BulkUploadId}], update result status [{result.Status}].", ex);
                    Unlock();
                }
                catch (Exception innerEx)
                {
                    _logger.Error($"An Exception occurred when trying to set FATAL status on bulkUpload. requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", innerEx);
                    throw;
                }

                throw;
            }

            return Task.CompletedTask;
        }

        private IEnumerable<DateTime> CalculateIngestDates(IEnumerable<BulkUploadResult> epgBulkUploadResults)
        {
            var allPrograms = epgBulkUploadResults.Where(p => p.Object != null).Select(p => p.Object as EpgProgramBulkUploadObject).ToList();
            var allProgramDates = allPrograms.Select(p => p.StartDate.Date).Distinct().ToList();

            allProgramDates.Add(allPrograms.Max(x => x.StartDate.Date).AddDays(1));
            allProgramDates.Add(allPrograms.Min(x => x.StartDate.Date).AddDays(-1));

            return allProgramDates;
        }

        private void InitHandlerProperties(BulkUploadTransformationEvent serviceEvent)
        {
            _eventData = serviceEvent;
            _locker = new DistributedLock(serviceEvent.GroupId);

            _bulUpload = BulkUploadMethods.GetBulkUploadData(serviceEvent.GroupId, serviceEvent.BulkUploadId);

            _jobData = _bulUpload.JobData as BulkUploadIngestJobData;
            if (_jobData == null) { throw new ArgumentException("bulUploadObject.JobData expected to be BulkUploadIngestJobData"); }

            _objectData = _bulUpload.ObjectData as BulkUploadEpgAssetData;
            if (_objectData == null) { throw new ArgumentException("bulUploadObject.ObjectData expected to be BulkUploadEpgAssetData"); }

            _ingestProfile = GetIngestProfile();
            _bulUpload.UpdaterId = serviceEvent.UserId;
            
            _languages = BulkUploadMethods.GetGroupLanguages(_eventData.GroupId, out _defaultLanguage);
            _languagesInfo = new LanguagesInfo
            {
                Languages = _languages,
                DefaultLanguage = _defaultLanguage
            };
        }

        private Status ValidateBulkUpload()
        {
            if (_bulUpload.JobData != null && _bulUpload.ObjectData != null)
            {
                return Status.Ok;
            }

            // else update the bulk upload object that an error occured
            _logger.Error($"ProcessBulkUpload cannot Deserialize file because JobData or ObjectData are null for groupId:{_bulUpload.GroupId}, bulkUploadId:{_bulUpload.Id}.");
            _bulUpload.AddError(eResponseStatus.Error, $"Error validate bulk upload. groupId: {_bulUpload.GroupId}, bulkUploadId: {_bulUpload.Id}");
            UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus.Fatal);
            return Status.Error;
        }

        public bool DeserializeAndSetResults()
        {
            // before start set the results list to an empty list to avoid null ref errors
            _bulUpload.Results = new List<BulkUploadResult>();

            var profile = IngestProfileManager.GetIngestProfileById(_bulUpload.GroupId, _jobData.IngestProfileId)?.Object;
            if (profile == null)
            {
                _bulUpload.AddError(eResponseStatus.IngestProfileNotExists, "Ingest Profile does not exist.");
                return false;
            }

            var xmlTvString = GetXmlTv(_bulUpload.FileURL, profile);

            try
            {
                if (string.IsNullOrEmpty(xmlTvString))
                {
                    _bulUpload.AddError(eResponseStatus.FileDoesNotExists, $"Could not find file:[{_bulUpload.FileURL}]");
                    return false;
                }

                var epgBulkUploadResults = DeserializeXmlTvEpgData(_bulUpload.Id, xmlTvString);


                foreach (var result in epgBulkUploadResults)
                {
                    var epgObject = result.Object as EpgProgramBulkUploadObject;
                    ProgramValidator.Validate(epgObject, result, _defaultLanguage);
                }

                if (epgBulkUploadResults.Any())
                {
                    _bulUpload.Results = epgBulkUploadResults.Cast<BulkUploadResult>().ToList();
                }

                if (epgBulkUploadResults.Any(r => r.Errors?.Any() == true))
                {
                    _bulUpload.AddError(eResponseStatus.Error, "Errors found during deserialization, review errors on result items.");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                _logger.Error($"Error during Epg ingest Deserialize", e);
                _bulUpload.AddError(eResponseStatus.Error, $"Unexpected error during Epg ingest Deserialize, ex:[{e.Message}]");
                return false;
            }
        }

        private static string GetXmlTv(string fileUrl, IngestProfile profile)
        {
            string xmlTvString;
            if (!string.IsNullOrEmpty(profile?.TransformationAdapterUrl))
            {
                _logger.Debug($"Found TransformationAdapterUrl:[{profile.TransformationAdapterUrl}] calling adapter to transform file");
                var transformationAdptr = new IngestTransformationAdapterClient(profile);
                xmlTvString = transformationAdptr.Transform(fileUrl);
            }
            else
            {
                _logger.Debug($"Transformation Adapter Url is not defined, assuming file is xmlTV format, downloading and parsing file.");
                xmlTvString = TryDownloadFileAsString(fileUrl);
            }

            return xmlTvString;
        }

        private static string TryDownloadFileAsString(string fileUrl)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    var xmlTvString = webClient.DownloadString(fileUrl);
                    return xmlTvString;
                }
                catch (Exception e)
                {
                    _logger.Error($"Error while downloading file to ingets, fileUrl:[{fileUrl}]", e);
                    return null;
                }
            }
        }

        private List<BulkUploadProgramAssetResult> DeserializeXmlTvEpgData(long bulkUploadId, string Data)
        {
            EpgChannels xmlTvEpgData;
            try
            {
                using (var textReader = new StringReader(Data))
                using (var xmlReader = XmlReader.Create(textReader))
                {
                    xmlTvEpgData = (EpgChannels)_XmlTVSerializer.Deserialize(xmlReader);
                }

                _logger.Debug($"DeserializeEpgChannel > Successfully  Deserialize xml. got epgchannels.programme.Length:[{xmlTvEpgData.programme.Length}]");
                // TODO: Arthur, Should we use this or the group id came with the builk request ?
                var groupId = xmlTvEpgData.groupid;
                var parentGroupId = xmlTvEpgData.parentgroupid;
                var epgPrograms = GetBulkUploadResults(bulkUploadId, parentGroupId, groupId, xmlTvEpgData);
                return epgPrograms;
            }
            catch (Exception ex)
            {
                _logger.Error("DeserializeEpgChannel > error while trying to Deserialize.", ex);
                throw;
            }
        }

        private List<BulkUploadProgramAssetResult> GetBulkUploadResults(long bulkUploadId, int parentGroupId, int groupId, EpgChannels xmlTvEpgData)
        {
            //var fieldEntityMapping = EpgIngest.Utils.GetMappingFields(parentGroupId);
            var channelExternalIds = xmlTvEpgData.channel.Select(s => s.id).ToList();

            _logger.Debug($"GetBulkUploadResults > Retriving kaltura channels for external IDs [{string.Join(",", channelExternalIds)}] ");
            var kalturaChannels = GetLinearChannelSettings(groupId, channelExternalIds);

            var response = new List<BulkUploadProgramAssetResult>();
            var programIndex = 0;
            foreach (var prog in xmlTvEpgData.programme)
            {
                var channelExternalId = prog.channel;
                // Every channel external id can point to mulitple interbal channels that have to have the same EPG
                // like channel per region or HD channel vs SD channel etc..
                var channelsToIngestProgramInto = kalturaChannels.Where(c => c.ChannelExternalID.Equals(channelExternalId, StringComparison.OrdinalIgnoreCase)).ToList();

                var programResults = new List<BulkUploadProgramAssetResult>();

                foreach (var innerChannel in channelsToIngestProgramInto)
                {
                    // TODO ARTHUR - WHY create results are here and not in BulkUploadEpgAssetData.GetNewBulkUploadResult like it should be?
                    var result = new BulkUploadProgramAssetResult
                    {
                        BulkUploadId = bulkUploadId,
                        Index = programIndex++,
                        ProgramExternalId = prog.external_id,
                        Status = BulkUploadResultStatus.InProgress,
                        LiveAssetId = innerChannel.LinearMediaId,
                        ChannelId = int.Parse(innerChannel.ChannelID),
                    };

                    result.StartDate = prog.ParseStartDate(result);
                    result.EndDate = prog.ParseEndDate(result);

                    result.Object = new EpgProgramBulkUploadObject
                    {
                        ParsedProgramObject = prog,
                        ChannelExternalId = innerChannel.ChannelExternalID,
                        ChannelId = int.Parse(innerChannel.ChannelID),
                        LinearMediaId = innerChannel.LinearMediaId,
                        GroupId = groupId,
                        ParentGroupId = parentGroupId,
                        StartDate = result.StartDate,
                        EndDate = result.EndDate,
                        EpgExternalId = prog.external_id,
                    };
                    programResults.Add(result);
                }

                // If there are no inner channels found the previous loop did not fill any results, than we add error results;
                if (!channelsToIngestProgramInto.Any())
                {
                    var result = new BulkUploadProgramAssetResult
                    {
                        BulkUploadId = bulkUploadId,
                        Index = programIndex++,
                        ProgramExternalId = prog.external_id,
                        Status = BulkUploadResultStatus.Error,
                        LiveAssetId = -1
                    };
                    var progrStartDate = prog.ParseStartDate(result);
                    var progrEnDate = prog.ParseEndDate(result);

                    result.Object = new EpgProgramBulkUploadObject
                    {
                        ParsedProgramObject = prog,
                        ChannelExternalId = string.Empty,
                        ChannelId = -1,
                        LinearMediaId = -1,
                        GroupId = groupId,
                        ParentGroupId = parentGroupId,
                        StartDate = progrStartDate,
                        EndDate = progrEnDate,
                        EpgExternalId = prog.external_id,
                    };

                    var msg = $"no channel was found for channelExternalId:[{channelExternalId}]";
                    result.AddError(eResponseStatus.ChannelDoesNotExist, msg);
                    programResults.Add(result);
                }

                response.AddRange(programResults);
            }

            return response;
        }

        // TODO: Take this from apiLogic after logic is fully converted
        public static List<LinearChannelSettings> GetLinearChannelSettings(int groupId, List<string> channelExternalIds)
        {
            var kalturaChannels = EpgDal.GetAllEpgChannelObjectsList(groupId, channelExternalIds);
            var kalturaChannelIds = kalturaChannels.Select(k => k.ChannelId).ToList();
            var liveAssets = CatalogDAL.GetLinearChannelSettings(groupId, kalturaChannelIds);
            return liveAssets;
        }



        private void EnqueueIngestEvents(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var ingestEvents = GenerateIngestEvents(crudOperations);
            if (!ingestEvents.Any())
            {
                _logger.Warn($"EnqueueIngestEvents > bulkUpload:[{_bulUpload.Id}], crudOperations:[{crudOperations}] resulted in an empty list, this might be due to policy set to CUT Source and the items to ingest were completely overlapped, so they were removed");
                Unlock();
                _bulUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Ok);
                UpdateBulkUpload(BulkUploadJobStatus.Success);
            }

            // in case the actual crud calculations are less thant the keys we locked initially this means we can unlock few days

            var effectiveLocIndices = ingestEvents.Select(e => e.TargetIndexName).ToList();
            var effectiveLockKeys = effectiveLocIndices.Select(targetIndexName => BulkUploadMethods.GetIngestLockKey(targetIndexName));
            var keysToUnlock = _jobData.LockKeys.Except(effectiveLockKeys);
            if (keysToUnlock.Any())
            {
                _logger.Info($"calculated crud operations did not include several days that were locked, unlocking:[{string.Join(",", keysToUnlock)}]");
                _locker.Unlock(keysToUnlock);
                var allCrudOperations = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.ItemsToDelete)
                .Concat(crudOperations.AffectedItems)
                .ToList();
                _jobData.DatesOfProgramsToIngest = allCrudOperations.Select(c=>c.StartDate.Date).Distinct().ToArray();
                _jobData.LockKeys = effectiveLockKeys.Distinct().ToArray();
                UpdateBulkUpload(_bulUpload.Status);
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
            _logger.Debug($"bulkUploadId:[{_bulUpload.Id}] calculated crud dates:[{string.Join(",", allCrudTargetIndices)}]");

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
            try
            {
                var epgV2Config = ApplicationConfiguration.Current.EPGIngestV2Configuration;
                var isLocked = _locker.Lock(_jobData.LockKeys,
                    epgV2Config.LockNumOfRetries.Value,
                    epgV2Config.LockRetryIntervalMS.Value,
                    epgV2Config.LockTTLSeconds.Value,
                    $"BulkUpload_{_bulUpload.Id}");
                if (!isLocked) { throw new Exception("Failed to acquire lock on ingest dates"); }
            }
            catch
            {
                Unlock();
                throw;
            }
        }

        private void Unlock()
        {
            _locker.Unlock(_jobData.LockKeys);
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
                setter(_bulUpload, items.Cast<IAffectedObject>().ToList());
            }
        }
        

        private void UpdateBulkUpload(BulkUploadJobStatus newStatus)
        {
            var result = BulkUploadManager.UpdateBulkUpload(_bulUpload, newStatus);
            if (result.IsOkStatusCode()) TrySendIngestCompleted(newStatus);
        }
        
        private GenericResponse<BulkUpload> UpdateBulkUploadStatusAndErrors(BulkUploadJobStatus newStatus)
        {
            var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulUpload, newStatus);
            if (result.IsOkStatusCode()) TrySendIngestCompleted(newStatus);

            return result;
        }

        private void TrySendIngestCompleted(BulkUploadJobStatus newStatus)
        {
            if (!BulkUpload.IsProcessCompletedByStatus(newStatus)) return;
            
            var updateDate = DateTime.UtcNow; // TODO looks like _bulUpload.UpdateDate is not updated in CB
            _epgIngestMessaging.EpgIngestCompleted(_bulUpload.GroupId, _bulUpload.UpdaterId,
                _bulUpload.Id, newStatus, _bulUpload.Errors, updateDate);
        }
    }
}