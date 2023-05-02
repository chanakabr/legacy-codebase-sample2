using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiLogic;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiLogic.Notification.Managers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Epg;
using ApiObjects.EventBus;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.CatalogManagement.Services;
using Core.GroupManagers;
using CouchbaseManager;
using EpgBL;
using EventBus.Abstraction;
using FeatureFlag;
using IngestHandler.Common;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Locking;
using IngestHandler.Domain.IngestProtection;
using Ott.Lib.FeatureToggle;
using Phx.Lib.Log;
using Synchronizer;
using Tvinci.Core.DAL;
using TVinciShared;

namespace IngestHandler
{
    public class IngestV2Handler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly CouchbaseManager.CouchbaseManager _couchbaseManager;

        private TvinciEpgBL _epgBL;
        private BulkUploadIngestEvent _eventData;
        private BulkUpload _bulkUpload;
        private BulkUploadResultsDictionary _relevantResultsDictionary;
        private IDictionary<string, LanguageObj> _languages;
        private LanguageObj _defaultLanguage;

        private List<EpgProgramBulkUploadObject> _allRelevantPrograms;
        private Dictionary<string, EpgCB> _autoFillEpgsCb;
        private Lazy<IReadOnlyDictionary<long, List<int>>> _linearChannelToRegionsMap;
        private string _logPrefix;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IIngestProtectProcessor _ingestProtectProcessor;
        private readonly ICatalogManagerAdapter _catalogManagerAdapter;
        private readonly IEpgAssetMultilingualMutator _epgAssetMultilingualMutator;
        private readonly IRegionManager _regionManager;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private readonly IIngestFinalizer _ingestFinalizer;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;

        public IngestV2Handler(
            IIndexManagerFactory indexManagerFactory,
            IIngestProtectProcessor ingestProtectProcessor,
            ICatalogManagerAdapter catalogManagerAdapter,
            IEpgAssetMultilingualMutator epgAssetMultilingualMutator,
            IRegionManager regionManager,
            IEpgIngestMessaging epgIngestMessaging,
            IIngestFinalizer ingestFinalizer,
            IPhoenixFeatureFlag phoenixFeatureFlag)
        {
            _indexManagerFactory = indexManagerFactory;
            _ingestProtectProcessor = ingestProtectProcessor;
            _catalogManagerAdapter = catalogManagerAdapter;
            _couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);
            _epgAssetMultilingualMutator = epgAssetMultilingualMutator
                                           ?? throw new ArgumentNullException(nameof(epgAssetMultilingualMutator));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _epgIngestMessaging = epgIngestMessaging;
            _ingestFinalizer = ingestFinalizer;
            _phoenixFeatureFlag = phoenixFeatureFlag;
        }

        public async Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                if (serviceEvent.TargetIndexName == null)
                {
                    _logger.Warn($"received event without TargetIndexName, assuming older version of transformation sent the message, trying to pares DateOfProgramsToIngest:{serviceEvent.DateOfProgramsToIngest}");
                    if (serviceEvent.DateOfProgramsToIngest == null) throw new Exception("could not parse target index  DateOfProgramsToIngest and TargetIndexName are null and");
                    serviceEvent.TargetIndexName = NamingHelper.Instance.GetDailyEpgIndexName(serviceEvent.GroupId, serviceEvent.DateOfProgramsToIngest);
                }

                _logger.Info($"Starting ingest write handler BulkUploadId: [{serviceEvent.BulkUploadId}], TargetIndexName:[{serviceEvent.TargetIndexName}], BulkUploadId:[{serviceEvent.BulkUploadId}], crud operations: [{serviceEvent.CrudOperations}]");
                await HandleIngestCrudOperations(serviceEvent);
            }
            finally
            {
                // unlock this day since it was refreshed and ingested
                var lockKeyOfThisDay = BulkUploadMethods.GetIngestLockKey(serviceEvent.TargetIndexName);
                var lockerMetadata = new Dictionary<string, string>
                {
                    { "BulkUploadId", serviceEvent.BulkUploadId.ToString() },
                    { "TargetIndexName", serviceEvent.TargetIndexName}
                };
                var locker = new DistributedLock(new LockContext(serviceEvent.GroupId, serviceEvent.UserId), _phoenixFeatureFlag, lockerMetadata);
                _logger.Info($"HandleIngestCrudOperations completed, unlocking current TargetIndexName:[{serviceEvent.TargetIndexName}], BulkUploadId: [{_eventData.BulkUploadId}]");
                locker.Unlock(new[] { lockKeyOfThisDay }, LockInitiator.GetBulkUploadLockInitiator(serviceEvent.BulkUploadId));
            }
        }

        private async Task HandleIngestCrudOperations(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                InitializedHandlerProperties(serviceEvent);
                SetProgramsWithEpgIds(serviceEvent.CrudOperations);
                AddEpgCBObjects(serviceEvent.CrudOperations);
                SetResultsWithObjectId(serviceEvent.CrudOperations);

                // validate no errors or fail
                // TODO very strange condition and block of code
                if (_bulkUpload.Results.Any(r => r.Errors?.Any() == true))
                {
                    _bulkUpload.AddError(eResponseStatus.Error, "errors while trying to parse/map epg objects, see results for details");

                    // set errors on all other items that are not errors of same date so that status can be finlized as failed
                    foreach (var r in _bulkUpload.Results)
                    {
                        if (r.Status != BulkUploadResultStatus.Error)
                        {
                            r.AddError(eResponseStatus.Error, $"Item was not ingested due to errors in other items in same TargetIndexName:[{_eventData.TargetIndexName}]");

                        }
                    }
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, BulkUploadJobStatus.Failed);
                    SendIngestPartCompleted(serviceEvent);
                    _logger.Debug($"{nameof(IngestV2Handler)} requestId:[{_eventData.RequestId}], BulkUploadId:[{_eventData.BulkUploadId}], update result status [{BulkUploadJobStatus.Failed}].");
                    return;
                }

                //update bulk object with the epgId set to all results
                UpdateBulkUploadObjectStatusAndResults();

                await UploadEpgImages(serviceEvent.CrudOperations);

                string dailyEpgIndexName = string.Empty;

                var indexManager = _indexManagerFactory.GetIndexManager(serviceEvent.GroupId);
                try
                {
                    dailyEpgIndexName = indexManager.SetupEpgV2Index(serviceEvent.TargetIndexName);
                }
                catch
                {
                    _bulkUpload.AddError(eResponseStatus.Error, "Error while building new index.");
                    throw new Exception($"index creation failed");
                }

                await BulkUploadMethods.UpdateCouchbase(serviceEvent.CrudOperations, serviceEvent.GroupId);

                indexManager.DeletePrograms(serviceEvent.CrudOperations.ItemsToDelete, NamingHelper.GetEpgIndexAlias(serviceEvent.GroupId), _languages);
                var programsToIndex = serviceEvent.CrudOperations.ItemsToAdd.Concat(serviceEvent.CrudOperations.ItemsToUpdate).Concat(serviceEvent.CrudOperations.AffectedItems).ToList();
                indexManager.UpsertPrograms(programsToIndex, dailyEpgIndexName, _defaultLanguage, _languages);

                await _ingestFinalizer.FinalizeEpgV2Ingest(serviceEvent, _bulkUpload, _relevantResultsDictionary);

                //create list of CRUD messages, delete + upsert 

                _logger.Info($"{_logPrefix} Ingest Handler completed.");
            }
            catch (Exception ex)
            {
                _logger.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}] targetIndexName:[{serviceEvent.TargetIndexName}].", ex);
                try
                {
                    _logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{serviceEvent.BulkUploadId}] targetIndexName:[{serviceEvent.TargetIndexName}]", ex);
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _logger.Error($"Trying to set fatal status on BulkUploadId:[{serviceEvent.BulkUploadId}] targetIndexName:[{serviceEvent.TargetIndexName}].", ex);
                    _bulkUpload.AddError(eResponseStatus.Error, $"An unexpected error occured during ingest, {ex.Message}");
                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, BulkUploadJobStatus.Fatal);
                    if (result.IsOkStatusCode()) TrySendIngestCompleted(BulkUploadJobStatus.Fatal);
                    SendIngestPartCompleted(serviceEvent);
                    _logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler requestId:[{_eventData.RequestId}], BulkUploadId:[{_eventData.BulkUploadId}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    _logger.Error($"An Exception occurred when trying to set FATAL status on bulkUpload. requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}] targetIndexName:[{serviceEvent.TargetIndexName}].", innerEx);
                    throw;
                }

                throw;
            }
        }

        private void SendIngestPartCompleted(BulkUploadIngestEvent serviceEvent)
        {
            var programs = serviceEvent.CrudOperations.ItemsToAdd
                .Concat(serviceEvent.CrudOperations.ItemsToUpdate)
                .Concat(serviceEvent.CrudOperations.ItemsToDelete)
                .Concat(serviceEvent.CrudOperations.AffectedItems)
                .ToList();
            var programIngestResults = _bulkUpload.ConstructResultsDictionary(programs)
                .Values
                .SelectMany(x => x.Values)
                .ToArray();
            var hasMoreToIngest = !BulkUpload.IsProcessCompletedByStatus(_bulkUpload.Status);
            var parameters = new EpgIngestPartCompletedParameters
            {
                BulkUploadId = _bulkUpload.Id,
                GroupId = _bulkUpload.GroupId,
                HasMoreEpgToIngest = hasMoreToIngest,
                UserId = _bulkUpload.UpdaterId,
                Results = programIngestResults
            };

            _epgIngestMessaging.EpgIngestPartCompleted(parameters);
        }

        private void SetResultsWithObjectId(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var allPrograms = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate).Where(p => !p.IsAutoFill);
            foreach (var prog in allPrograms)
            {
                _relevantResultsDictionary[prog.ChannelId][prog.EpgExternalId].ObjectId = (long)prog.EpgId;
            }
        }

        private void InitializedHandlerProperties(BulkUploadIngestEvent serviceEvent)
        {
            _logger.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
            _epgBL = new TvinciEpgBL(serviceEvent.GroupId);
            _eventData = serviceEvent;
            _logPrefix = $"BulkUploadId [{_eventData.BulkUploadId}], targetIndexName:[{serviceEvent.TargetIndexName}] >"; // TODO should be contextual logging
            ValidateServiceEvent();
            _bulkUpload = BulkUploadMethods.GetBulkUploadData(serviceEvent.GroupId, serviceEvent.BulkUploadId);
            if (_bulkUpload.Results?.Any() != true) { throw new Exception("received bulk upload without any crud operations"); }
            _languages = BulkUploadMethods.GetGroupLanguages(_eventData.GroupId, out _defaultLanguage);
            _allRelevantPrograms = _eventData.CrudOperations.ItemsToAdd
                .Concat(_eventData.CrudOperations.ItemsToUpdate)
                .Concat(_eventData.CrudOperations.ItemsToDelete)
                .Concat(_eventData.CrudOperations.AffectedItems)
                .ToList();
            _relevantResultsDictionary = _bulkUpload.ConstructResultsDictionary(_allRelevantPrograms);

            _autoFillEpgsCb = EpgPartnerConfigurationManager.Instance.GetAutofillTemplate(serviceEvent.GroupId);

            _linearChannelToRegionsMap = new Lazy<IReadOnlyDictionary<long, List<int>>>(
                () => _regionManager.GetLinearMediaToRegionsMapWhenEnabled(_eventData.GroupId));
        }

        private void ValidateServiceEvent()
        {
            _logger.Debug($"ValidateServiceEvent: _EventData.ProgramsToIngest.Count:[{_eventData.CrudOperations}]");
            if (!_eventData.CrudOperations.ItemsToAdd.Any() &&
                !_eventData.CrudOperations.ItemsToDelete.Any() &&
                !_eventData.CrudOperations.ItemsToUpdate.Any() &&
                !_eventData.CrudOperations.AffectedItems.Any() &&
                !_eventData.CrudOperations.RemainingItems.Any()
            )
            {
                throw new Exception($"Received bulk upload ingest event with empty crud operations to insert. group id = {_eventData.GroupId} id = {_eventData.BulkUploadId}");
            }
        }

        private void UpdateBulkUploadObjectStatusAndResults()
        {
            var resultsToUpdate = _relevantResultsDictionary.Values.SelectMany(r => r.Values).ToList();
            BulkUploadManager.UpdateBulkUploadResults(resultsToUpdate, out var jobStatus);

            _logger.Debug($"UpdateBulkUploadObjectStatusAndResults > updated results, calculated status by results: [{jobStatus}]");
            BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_bulkUpload, jobStatus);
        }

        private async Task UploadEpgImages(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var newAndUpdatedPrograms = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.AffectedItems)
                .ToList();
            var pics = newAndUpdatedPrograms.SelectMany(p => p.EpgCbObjects).SelectMany(p => p.pictures).ToList();
            _ = await EpgImageManager.UploadEPGPictures(_eventData.GroupId, pics);
            // TODO: arthur: should be do something with the results of the uploaded images ?
        }

        private void AddEpgCBObjects(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            _logger.Debug($"Generating EpgCB translation object for every bulk request, with languages:[{string.Join(",", _languages.Keys)}]");

            var addAndUpdateProgs = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate);
            foreach (var prog in addAndUpdateProgs)
            {
                prog.EpgCbObjects = new List<EpgCB>();
                if (prog.IsAutoFill)
                {
                    var epgItems = GetAutoFillEpgCBDocuments(prog);
                    prog.EpgCbObjects.AddRange(epgItems.Values);
                }
                else
                {
                    foreach (var lang in _languages.Values)
                    {
                        var progResult = _relevantResultsDictionary[prog.ChannelId][prog.EpgExternalId];
                        var epgItem = GenerateEpgCBObject(lang.Code,
                            _defaultLanguage.Code,
                            prog,
                            progResult,
                            _epgAssetMultilingualMutator.IsAllowedToFallback(_eventData.GroupId, _languages));
                        prog.EpgCbObjects.Add(epgItem);
                    }
                }
            }

            var doesGroupUsesTemplates = _catalogManagerAdapter.DoesGroupUsesTemplates(_bulkUpload.GroupId);
            if (doesGroupUsesTemplates)
            {
                _ingestProtectProcessor.ProcessIngestProtect(_eventData.GroupId, crudOperations);
            }

            // add existing documentIds to affectedItems
            // affected items are the programs that got Cut due to overlap.
            // we dont have their data from the ingest file so we need to go to ES and CB to get their existing data and fill their EPG CB
            // it is also important to remeber to update the start \ end of the CB docs as they are the ones who get indexed into ES
            if (crudOperations.AffectedItems.Any())
            {
                var documentIds = _epgBL.GetEpgsCBKeys(_eventData.GroupId, crudOperations.AffectedItems.Select(x => (long)x.EpgId), _languages.Values, false);
                var epgCbList = EpgDal.GetEpgCBList(documentIds);
                foreach (var affectedProgram in crudOperations.AffectedItems)
                {
                    affectedProgram.EpgCbObjects = epgCbList.Where(x => x.EpgID == affectedProgram.EpgId).ToList();
                    affectedProgram.EpgCbObjects.ForEach(e => e.StartDate = affectedProgram.StartDate);
                    affectedProgram.EpgCbObjects.ForEach(e => e.EndDate = affectedProgram.EndDate);
                }
            }
        }

        private void SetProgramsWithEpgIds(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            _logger.Debug($"CalculateSimulatedFinalStateAfterIngest > adding EpgIds to new programs");
            if (crudOperations.ItemsToAdd.Any())
            {
                var newIds = _epgBL.GetNewEpgIds(crudOperations.ItemsToAdd.Count).ToList();
                for (int i = 0; i < crudOperations.ItemsToAdd.Count; i++)
                {
                    var idToSet = newIds[i];
                    crudOperations.ItemsToAdd[i].EpgId = idToSet;
                }
            }
        }

        private EpgCB GenerateEpgCBObject(
            string langCode,
            string defaultLangCode,
            EpgProgramBulkUploadObject prog,
            BulkUploadProgramAssetResult bulkUploadResultItem,
            bool isMultilingualFallback)
        {
            var epgItem = new EpgCB();
            var parsedProg = prog.ParsedProgramObject;

            epgItem.DocumentId = GetEpgCBDocumentId(prog.EpgId, bulkUploadResultItem.BulkUploadId, langCode);
            epgItem.Language = langCode;
            epgItem.ChannelID = prog.ChannelId;
            epgItem.LinearMediaId = prog.LinearMediaId;
            epgItem.GroupID = prog.GroupId;
            epgItem.ParentGroupID = prog.ParentGroupId;
            epgItem.EpgID = prog.EpgId;
            epgItem.EpgIdentifier = prog.EpgExternalId;
            epgItem.StartDate = prog.StartDate;
            epgItem.EndDate = prog.EndDate;
            epgItem.UpdateDate = DateTime.UtcNow;
            epgItem.CreateDate = DateTime.UtcNow;
            epgItem.IsActive = true;
            epgItem.Status = 1;
            epgItem.EnableCatchUp = XmlTvParsingHelper.ParseXmlTvEnableStatusValue(parsedProg.enablecatchup);
            epgItem.EnableCDVR = XmlTvParsingHelper.ParseXmlTvEnableStatusValue(parsedProg.enablecdvr);
            epgItem.EnableStartOver = XmlTvParsingHelper.ParseXmlTvEnableStatusValue(parsedProg.enablestartover);
            epgItem.EnableTrickPlay = XmlTvParsingHelper.ParseXmlTvEnableStatusValue(parsedProg.enabletrickplay);
            epgItem.Crid = parsedProg.crid;
            epgItem.IsIngestV2 = true;

            epgItem.Name = parsedProg.title.GetTitleByLanguage(langCode, defaultLangCode, out var nameParsingStatus);
            if (nameParsingStatus != eResponseStatus.OK)
            {
                bulkUploadResultItem.AddError(nameParsingStatus, $"Error parsing title for programExternalId:[{parsedProg.external_id}], langCode:[{langCode}], defaultLang:[{defaultLangCode}]");
            }

            epgItem.Description = parsedProg.desc.GetDescriptionByLanguage(langCode, defaultLangCode, out var descriptionParsingStatus);
            if (descriptionParsingStatus != eResponseStatus.OK)
            {
                bulkUploadResultItem.AddError(nameParsingStatus, $"Error parsing description for programExternalId:[{parsedProg.external_id}], langCode:[{langCode}], defaultLang:[{defaultLangCode}]");
            }

            epgItem.Metas = parsedProg.ParseMetas(langCode, defaultLangCode, bulkUploadResultItem, isMultilingualFallback);
            epgItem.Tags = parsedProg.ParseTags(langCode, defaultLangCode, bulkUploadResultItem, isMultilingualFallback);
            epgItem.regions = GetRegions(epgItem.LinearMediaId);

            epgItem.ExternalOfferIds = parsedProg.ParseExternalOfferIds(langCode, defaultLangCode, bulkUploadResultItem);

            var suppressesIndexes = CatalogPartnerConfigManager.Instance.GetMediaSuppressedIndexes(prog.GroupId)?.Object;
            epgItem.Suppressed = IndexManagerCommonHelpers.GetSuppressedIndex(epgItem, suppressesIndexes);
            PrepareEpgItemImages(parsedProg.icon, epgItem);

            return epgItem;
        }

        private static string GetEpgCBDocumentId(ulong epgId, long bulkUploadId, string langCode)
        {
            return $"epg_{bulkUploadId}_{langCode}_{epgId}";
        }

        private void PrepareEpgItemImages(icon[] icons, EpgCB epgItem)
        {
            if (icons?.Any() != true)
            {
                _logger.Info($"Program with external Id:[{epgItem.EpgIdentifier}] has no images to upload");
                return;
            }

            epgItem.pictures = epgItem.pictures ?? new List<EpgPicture>();
            var groupRatioNamesToImageTypes = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(_eventData.GroupId);
            var nonOpcGroupRatios = EpgDal.Get_PicsEpgRatios();
            var isOpc = GroupSettingsManager.Instance.IsOpc(_eventData.GroupId);
            foreach (var icon in icons)
            {
                var epgPicture = new EpgPicture();
                var imgUrl = icon.src;
                long ratioId = 0;
                long imageTypeId = 0;


                if (isOpc)
                {
                    if (groupRatioNamesToImageTypes.TryGetValue(icon.ratio, out var imgType))
                    {
                        imageTypeId = imgType.Id;
                        if (imgType.RatioId.HasValue)
                        {
                            ratioId = imgType.RatioId.Value;
                        }
                        else
                        {
                            throw new Exception($"opc partner must have ratioId defined for ration:[{icon.ratio}], programExternalId:[{epgItem.EpgIdentifier}], icon.src:[{icon.src}]");
                        }
                    }
                }
                else
                {
                    ratioId = long.Parse(nonOpcGroupRatios.FirstOrDefault(r => r.Value == icon.ratio).Key);
                }

                epgPicture.Url = imgUrl;
                epgPicture.PicID = -1;
                epgPicture.Ratio = icon.ratio;
                epgPicture.RatioId = (int)ratioId;
                epgPicture.ImageTypeId = imageTypeId;
                epgPicture.ProgramName = epgItem.Name;
                epgPicture.ChannelId = epgItem.ChannelID;
                epgPicture.SourceUrl = imgUrl;
                epgItem.pictures.Add(epgPicture);

                //if (!string.IsNullOrEmpty(imgUrl))
                //{
                //    int picId = EpgImageManager.DownloadEPGPic(imgUrl, epgItem.Name, epgItem.GroupID, 0, epgItem.ChannelID, ratioId, imageTypeId);
                //    if (picId != 0)
                //    {
                //        var baseURl = ODBCWrapper.Utils.GetTableSingleVal("epg_pics", "BASE_URL", picId);
                //        if (baseURl != null && baseURl != DBNull.Value)
                //        {
                //            epgPicture.Url = baseURl.ToString();
                //            epgPicture.PicID = picId;
                //            epgPicture.Ratio = icon.ratio;
                //            epgPicture.ImageTypeId = imageTypeId;
                //            epgItem.pictures.Add(epgPicture);
                //        }
                //    }
                //    else
                //    {
                //        bulkUploadResultItem.AddWarning((int)IngestWarnings.FailedDownloadPic, "Failed to download Epg picture");
                //    }
                //}
            }
        }

        private Dictionary<string, EpgCB> GetAutoFillEpgCBDocuments(EpgProgramBulkUploadObject prog)
        {
            var autofillDocs = new Dictionary<string, EpgCB>();
            foreach (var doc in _autoFillEpgsCb)
            {
                autofillDocs[doc.Key] = ObjectCopier.Clone(doc.Value);

                autofillDocs[doc.Key].EpgID = prog.EpgId;
                autofillDocs[doc.Key].DocumentId = GetEpgCBDocumentId(prog.EpgId, _bulkUpload.Id, doc.Key);
                autofillDocs[doc.Key].EpgIdentifier = prog.EpgExternalId;
                autofillDocs[doc.Key].ChannelID = prog.ChannelId;
                autofillDocs[doc.Key].LinearMediaId = prog.LinearMediaId;
                autofillDocs[doc.Key].GroupID = prog.GroupId;
                autofillDocs[doc.Key].ParentGroupID = prog.ParentGroupId;
                autofillDocs[doc.Key].StartDate = prog.StartDate;
                autofillDocs[doc.Key].EndDate = prog.EndDate;
                autofillDocs[doc.Key].EnableCatchUp = 0;
                autofillDocs[doc.Key].EnableCDVR = 0;
                autofillDocs[doc.Key].EnableStartOver = 0;
                autofillDocs[doc.Key].EnableTrickPlay = 0;
                autofillDocs[doc.Key].CreateDate = DateTime.UtcNow;
                autofillDocs[doc.Key].UpdateDate = DateTime.UtcNow;
                autofillDocs[doc.Key].regions = GetRegions(prog.LinearMediaId);
            }

            return autofillDocs;
        }

        private List<int> GetRegions(long linearMediaId)
        {
            return _linearChannelToRegionsMap.Value.TryGetValue(linearMediaId, out var regions)
                ? regions
                : null;
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
