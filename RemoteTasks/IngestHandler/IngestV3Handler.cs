using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiLogic;
using ApiLogic.Api.Managers;
using ApiLogic.Catalog.CatalogManagement.Helpers;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.EPG;
using ApiLogic.IndexManager.Helpers;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement.Services;
using Core.GroupManagers;
using CouchbaseManager;
using EpgBL;
using FeatureFlag;
using IngestHandler.Common;
using Ingesthandler.common.Generated.Api.Events.ChannelIngestStaged;
using Ingesthandler.common.Generated.Api.Events.UpdateBulkUpload;
using IngestHandler.Common.Infrastructure;
using IngestHandler.Common.Managers;
using IngestHandler.Common.Managers.Abstractions;
using IngestHandler.Common.Repositories;
using IngestHandler.Domain.IngestProtection;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using OTT.Lib.Kafka.Extensions;
using Phx.Lib.Log;
using Tvinci.Core.DAL;
using TVinciShared;
using Utils = Core.Catalog.Utils;

namespace IngestHandler
{
    public class IngestV3Handler : IKafkaMessageHandler<ChannelIngestStaged>
    {
        private readonly IIngestStagingRepository _ingestStagingRepository;
        private readonly IEpgCRUDOperationsManager _crudOperationsManager;
        private readonly IIngestProfileRepository _ingestProfileRepository;
        private readonly IBulkUploadRepository _bulkUploadRepository;
        private readonly IBulkUploadService _bulkUploadService;
        private readonly IIndexManagerFactory _indexManagerFactory;
        private readonly IIngestProtectProcessor _ingestProtectProcessor;
        private readonly ICatalogManagerAdapter _catalogManagerAdapter;
        private readonly IEpgAssetMultilingualMutator _epgAssetMultilingualMutator;
        private readonly IRegionManager _regionManager;
        private readonly IEpgIngestMessaging _epgIngestMessaging;
        private readonly IIngestFinalizer _ingestFinalizer;
        private readonly IPhoenixFeatureFlag _phoenixFeatureFlag;
        private readonly ILogger<IngestV3Handler> _logger;
        private readonly IKafkaProducerFactory _kafkaProducerFactory;
        private readonly IKafkaContextProvider _kafkaContextProvider;
        private readonly IKafkaProducer<string, UpdateBulkUpload> _kafkaUpdateBulkUploadProducer;
        private readonly CouchbaseManager.CouchbaseManager _couchbaseManager;
        private string _logPrefix;
        private TvinciEpgBL _epgBL;
        private BulkUpload _bulkUpload;
        private BulkUploadResultsDictionary _relevantResultsDictionary;
        private List<EpgProgramBulkUploadObject> _allRelevantPrograms;
        private Dictionary<string, EpgCB> _autoFillEpgsCb;
        private Lazy<IReadOnlyDictionary<long, List<int>>> _linearChannelToRegionsMap;
        private LanguagesInfo _languagesInfo;
        private CRUDOperations<EpgProgramBulkUploadObject> _crudOperations;
        private BulkUploadIngestJobData _jobData;
        private Dictionary<string, string> _nonOpcGroupRatios;
        private bool _isOpc;
        private Events.eEvent _kmonEvt;
        private string _partnerIdStr;
        private long _bulkUploadId;
        private long _linearChannelId;
        private Dictionary<string, ImageType> _groupRatioNamesToImageTypes;
        private int _partnerId;

        public IngestV3Handler(
            IIngestStagingRepository ingestStagingRepository,
            IBulkUploadRepository bulkUploadRepository,
            IBulkUploadService bulkUploadService,
            IEpgCRUDOperationsManager crudOperationsManager,
            IIngestProfileRepository ingestProfileRepository,
            IIndexManagerFactory indexManagerFactory,
            IIngestProtectProcessor ingestProtectProcessor,
            ICatalogManagerAdapter catalogManagerAdapter,
            IEpgAssetMultilingualMutator epgAssetMultilingualMutator,
            IRegionManager regionManager,
            IEpgIngestMessaging epgIngestMessaging,
            IIngestFinalizer ingestFinalizer,
            IPhoenixFeatureFlag phoenixFeatureFlag,
            IKafkaProducerFactory kafkaProducerFactory,
            IKafkaContextProvider kafkaContextProvider,
            ILogger<IngestV3Handler> logger)
        {
            _ingestStagingRepository = ingestStagingRepository;
            _bulkUploadRepository = bulkUploadRepository;
            _bulkUploadService = bulkUploadService;
            _crudOperationsManager = crudOperationsManager;
            _ingestProfileRepository = ingestProfileRepository;
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
            _logger = logger;

            _kafkaProducerFactory = kafkaProducerFactory;
            _kafkaContextProvider = kafkaContextProvider;

            _kafkaUpdateBulkUploadProducer = _kafkaProducerFactory.Get<string, UpdateBulkUpload>(_kafkaContextProvider, Confluent.Kafka.Partitioner.Consistent);
        }

        public async Task<HandleResult> Handle(ConsumeResult<string, ChannelIngestStaged> consumeResult)
        {
            Func<Task> completeLinearChannelOfBulkUpload = null;
            try
            {
                var msg = consumeResult.Result.Message.Value;
                // TODO: is there a reason for these fields being nullable ? 
                if (!msg.PartnerId.HasValue || !msg.BulkUploadId.HasValue || !msg.LinearChannelId.HasValue)
                {
                    _logger.LogError($"received event with missing information. partnerId:[{msg.PartnerId}], bulkUploadId:[{msg.BulkUploadId}], linearChannelId:[{msg.LinearChannelId}]");
                    return new HandleResult();
                }

                var partnerId = (int)msg.PartnerId.Value;
                var bulkUploadId = msg.BulkUploadId.Value;
                var linearChannelId = msg.LinearChannelId.Value;

                _logger.LogInformation($"Starting IngestHandler v3, bulkUploadId:[{bulkUploadId}], channelId:[{linearChannelId}], partner:[{partnerId}]");

                var shouldProcessBulkUpload = await _bulkUploadService.ShouldProcessLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);
                if (shouldProcessBulkUpload == false)
                {
                    _logger.LogWarning($"Interrupted IngestHandler v3. This bulkUpload has already been taken on process or finished. Repeated process has been skipped, bulkUploadId:[{bulkUploadId}], channelId:[{linearChannelId}], partner:[{partnerId}]");
                }
                else
                {
                    completeLinearChannelOfBulkUpload = () => _bulkUploadRepository.CompleteLinearChannelOfBulkUpload(partnerId, bulkUploadId, linearChannelId);
                    await IngestChannelPrograms(partnerId, bulkUploadId, linearChannelId);
                    _logger.LogInformation($"Completed IngestHandler v3, bulkUploadId:[{bulkUploadId}], channelId:[{linearChannelId}], partner:[{partnerId}]");
                }
            }
            catch (Exception ex)
            {
                var resValue = consumeResult.Result.Message.Value;
                _logger.LogError(ex, $"An Exception occurred in BulkUploadIngestHandler, BulkUploadId:[{resValue.BulkUploadId}] LinearChannelId:[{resValue.LinearChannelId}].");
                try
                {
                    _logger.LogError(ex, $"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{resValue.BulkUploadId}]  LinearChannelId:[{resValue.LinearChannelId}]");
                    _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _logger.LogError(ex, $"Trying to set fatal status on BulkUploadId:[{resValue.BulkUploadId}]  LinearChannelId:[{resValue.LinearChannelId}].");

                    // this message states "during transformation handler" and we wont change for now because e2e automation verifies the text of the error message.
                    _bulkUpload.AddError(eResponseStatus.Error, $"An unexpected error occurred during ingest handler, {ex.Message}");

                    await ProduceUpdateBulkUpload(_bulkUpload.GroupId, _bulkUpload.Id, shouldSetAsFatal:true, errors: _bulkUpload.Errors);
                    TrySendIngestCompleted(BulkUploadJobStatus.Fatal);
                    SendIngestPartCompleted(_crudOperations);
                    _logger.LogError(ex, $"An Exception occurred in BulkUploadIngestValidationHandler, BulkUploadId:[{resValue.BulkUploadId}].");
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, $"An Exception occurred when trying to set FATAL status on bulkUpload., BulkUploadId:[{resValue.BulkUploadId}] LinearChannelId:[{resValue.LinearChannelId}].");
                }
            }
            finally
            {
                if (completeLinearChannelOfBulkUpload != null)
                {
                    await completeLinearChannelOfBulkUpload();
                }
            }

            return new HandleResult();
        }

        private void InitHandlerProperties(int partnerId, long bulkUploadId, long linearChannelId)
        {
            _kmonEvt = Phx.Lib.Log.Events.eEvent.EVENT_WS;
            _partnerIdStr = partnerId.ToString();
            _partnerId = partnerId;
            _bulkUploadId = bulkUploadId;
            _linearChannelId = linearChannelId;
            using var km = CreateKMonitor("InitHandlerProperties");
            _logger.LogDebug($"Starting BulkUploadIngestHandler  BulkUploadId:[{bulkUploadId}]");
            // init to avoid null ref exception
            _crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            _epgBL = new TvinciEpgBL(partnerId);
            _logPrefix = $"[{(_partnerIdStr)}-{_bulkUpload}-{_linearChannelId}] >";
            _bulkUpload = BulkUploadMethods.GetBulkUploadData(partnerId, bulkUploadId);
            if (_bulkUpload.Results?.Any() != true) { throw new Exception("received bulk upload without any crud operations"); }

            var languages = BulkUploadMethods.GetGroupLanguages(partnerId, out var defaultLanguage);
            _languagesInfo = new LanguagesInfo
            {
                Languages = languages,
                DefaultLanguage = defaultLanguage
            };

            _jobData = _bulkUpload.JobData as BulkUploadIngestJobData;
            if (_jobData == null) { throw new Exception("bulUploadObject.JobData expected to be BulkUploadIngestJobData"); }

            _nonOpcGroupRatios = EpgDal.Get_PicsEpgRatios();
            _isOpc = GroupSettingsManager.Instance.IsOpc(partnerId);
            _groupRatioNamesToImageTypes = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(partnerId);
            _linearChannelToRegionsMap = new Lazy<IReadOnlyDictionary<long, List<int>>>(
                () => _regionManager.GetLinearMediaToRegionsMapWhenEnabled(partnerId));

            _autoFillEpgsCb = EpgPartnerConfigurationManager.Instance.GetAutofillTemplate(partnerId);
        }

        private async Task IngestChannelPrograms(int partnerId, long bulkUploadId, long linearChannelId)
        {
            InitHandlerProperties(partnerId, bulkUploadId, linearChannelId);
            await CalculateCrudOperations(partnerId, bulkUploadId, linearChannelId);
            if (_bulkUpload.Results.Any(r => r.Errors?.Any() == true))
            {
                _bulkUpload.AddError(eResponseStatus.Error, "error while trying to calculate required changes to Epg, see results for more information");
                _bulkUpload.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                SendIngestPartCompleted(_crudOperations);
                var results = _relevantResultsDictionary.Values.SelectMany(r => r.Values).ToList();
                await ProduceUpdateBulkUpload(partnerId, _bulkUploadId, resultsToUpdate: results);
                return;
            }

            SetProgramsWithMissedEpgIds(_crudOperations);
            SetCrudOperationToBulkUpload();
            SetResultsWithObjectId(_crudOperations);
            AddEpgCBObjects(_crudOperations);

            if (_bulkUpload.Results.Any(r => r.Errors?.Any() == true))
            {
                _bulkUpload.AddError(eResponseStatus.Error, "errors while trying to parse/map epg objects, see results for details");

                // set errors on all other items that are not errors of same date so that status can be finalized as failed
                foreach (var r in _bulkUpload.Results.Where(r => r.Status != BulkUploadResultStatus.Error))
                {
                    r.AddError(eResponseStatus.Error, $"Item was not ingested due to errors in other items in same  LinearChannelId:[{linearChannelId}]");
                }

                var results = _relevantResultsDictionary.Values.SelectMany(r => r.Values).ToList();
                await ProduceUpdateBulkUpload(partnerId, bulkUploadId, resultsToUpdate: results, errors: _bulkUpload.Errors);
                SendIngestPartCompleted(_crudOperations);
                _logger.LogDebug($"{nameof(IngestV3Handler)}, BulkUploadId:[{bulkUploadId}], update result status [{BulkUploadJobStatus.Failed}].");
                return;
            }

            //update bulk object with the epgId set to all results
            var resultsToUpdate = _relevantResultsDictionary.Values.SelectMany(r => r.Values).ToList();
            await ProduceUpdateBulkUpload(partnerId, bulkUploadId, resultsToUpdate: resultsToUpdate, crudOps: _crudOperations);
            await UploadEpgImages(_crudOperations);
            await UpdateCouchbase(partnerId);
            UpdateElasticsearch(partnerId);

            // Set OK to all results
            resultsToUpdate.ForEach(r => r.Status = BulkUploadResultStatus.Ok);
            await ProduceUpdateBulkUpload(partnerId, bulkUploadId, resultsToUpdate: resultsToUpdate);
            _logger.LogInformation($"{_logPrefix} Ingest Handler completed.");
            return;
        }

        private void UpdateElasticsearch(int partnerId)
        {
            using var km = CreateKMonitor("UpdateElasticsearch");
            var indexManager = _indexManagerFactory.GetIndexManager(partnerId);
            indexManager.SetupEpgV3Index();

            var transactionId = NamingHelper.GetEpgV3TransactionId(_linearChannelId, _bulkUploadId);
            var programsToIndex = _crudOperations.ItemsToAdd
                .Concat(_crudOperations.ItemsToUpdate)
                .Concat(_crudOperations.AffectedItems)
                .SelectMany(i => i.EpgCbObjects)
                .Select(p=> ObjectCopier.Clone(p))
                .ToList();

            // this list contains new programs to index, either completely new, or a new version of an update\aaffected operation
            // in both cases for transactional-visibility we need to create new doc ids
            programsToIndex.ForEach(p => p.DocumentId = GetEpgCBDocumentId(p.EpgID, _bulkUpload.Id, p.Language));
            
            var programsToDelete = _crudOperations.ItemsToDelete
                .SelectMany(i => i.EpgCbObjects)
                .ToList();
            
            indexManager.ApplyEpgCrudOperationWithTransaction(transactionId, programsToIndex, programsToDelete);
            indexManager.CommitEpgCrudTransaction(transactionId, _linearChannelId);

            // TODO: verify this is better than polling ES to verify all async writes have been made...
            // TODO: try to think of a dataloader style solution where we delay for few ms to consolidate flush requests ...
            var epgIndexAlias = NamingHelper.GetEpgIndexAlias(partnerId);
            indexManager.ForceRefreshEpgIndex(epgIndexAlias);
        }


        private async Task UpdateCouchbase(int partnerId)
        {
            using var km = CreateKMonitor("UpdateCouchbase");
            var crudsForCB = new CRUDOperations<EpgProgramBulkUploadObject>();
            crudsForCB.ItemsToAdd = _crudOperations.ItemsToAdd;
            crudsForCB.AffectedItems = _crudOperations.AffectedItems.Select(i => ObjectCopier.Clone(i)).ToList();
            crudsForCB.ItemsToDelete = _crudOperations.ItemsToDelete;
            crudsForCB.ItemsToUpdate = _crudOperations.ItemsToUpdate;
            crudsForCB.RemainingItems = _crudOperations.RemainingItems;

            // for epg v3 we have cloned all affected items (to avoid chaging same ref pointers in other lists)
            // then we re-generated new documentIds so that they can be added and deleted.
            crudsForCB.AffectedItems
                .SelectMany(i => i.EpgCbObjects)
                .ToList()
                .ForEach(p => p.DocumentId = GetEpgCBDocumentId(p.EpgID, _bulkUpload.Id, p.Language));
            await BulkUploadMethods.UpdateCouchbase(crudsForCB, partnerId);
        }

        private void SetCrudOperationToBulkUpload()
        {
            // objects are saved on the bulk upload object, because
            // they are also used by the ingest finalize for cache invalidation and update recordings quota
            _bulkUpload.AffectedObjects = _crudOperations.AffectedItems.Cast<IAffectedObject>().ToList();
            _bulkUpload.UpdatedObjects = _crudOperations.ItemsToUpdate.Cast<IAffectedObject>().ToList();
            _bulkUpload.DeletedObjects = _crudOperations.ItemsToDelete.Cast<IAffectedObject>().ToList();
            _bulkUpload.AddedObjects = _crudOperations.ItemsToAdd.Cast<IAffectedObject>().ToList();
        }

        private void SendIngestPartCompleted(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var programs = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.ItemsToDelete)
                .Concat(crudOperations.AffectedItems)
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
            using var km = CreateKMonitor("SetResultsWithObjectId");
            var allPrograms = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate).Where(p => !p.IsAutoFill);
            foreach (var prog in allPrograms)
            {
                _relevantResultsDictionary[prog.ChannelId][prog.EpgExternalId].ObjectId = (long)prog.EpgId;
            }
        }

        private async Task CalculateCrudOperations(int partnerId, long bulkUploadId, long channelId)
        {
            using var km = CreateKMonitor("CalculateCrudOperations");
            var allRelevantPrograms = await _ingestStagingRepository.GetProgramsFromStagingCollection(partnerId, bulkUploadId, channelId);
            _allRelevantPrograms = allRelevantPrograms.ToList();

            var ingestProfile = _ingestProfileRepository.GetIngestProfile(partnerId, _jobData.IngestProfileId);

            _crudOperations = _crudOperationsManager.CalculateCRUDOperationsForChannel(_bulkUpload, (int)channelId, ingestProfile.DefaultOverlapPolicy, ingestProfile.DefaultAutoFillPolicy, _allRelevantPrograms, _languagesInfo);
            _relevantResultsDictionary = _bulkUpload.ConstructResultsDictionary(_allRelevantPrograms);
        }


        private async Task UploadEpgImages(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            using var km = CreateKMonitor("UploadEpgImages");
            var newAndUpdatedPrograms = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.AffectedItems)
                .ToList();
            var pics = newAndUpdatedPrograms.SelectMany(p => p.EpgCbObjects).SelectMany(p => p.pictures).ToList();
            _ = await EpgImageManager.UploadEPGPictures(_bulkUpload.GroupId, pics);
        }

        private void AddEpgCBObjects(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            using var km = CreateKMonitor("AddEpgCBObjects");
            _logger.LogDebug($"Generating EpgCB translation object for every bulk request, with languages:[{string.Join(",", _languagesInfo.Languages.Keys)}]");

            var addAndUpdateProgs = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate);
            // TODO: ask artsiom what is this ???
            var epgAssetMultilingualMutator_IsAllowedToFallback = _epgAssetMultilingualMutator.IsAllowedToFallback(_bulkUpload.GroupId, _languagesInfo.Languages);
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
                    foreach (var lang in _languagesInfo.Languages.Values)
                    {
                        var progResult = _relevantResultsDictionary[prog.ChannelId][prog.EpgExternalId];
                        var epgItem = GenerateEpgCBObject(lang.Code,
                            _languagesInfo.DefaultLanguage.Code,
                            prog,
                            progResult,
                            epgAssetMultilingualMutator_IsAllowedToFallback);
                        prog.EpgCbObjects.Add(epgItem);
                    }
                }
            }

            if (_isOpc)
            {
                _ingestProtectProcessor.ProcessIngestProtect(_bulkUpload.GroupId, crudOperations);
            }

            // add existing documentIds to affectedItems
            // affected items are the programs that got Cut due to overlap.
            // we dont have their data from the ingest file so we need to go to ES and CB to get their existing data and fill their EPG CB
            // it is also important to remember to update the start \ end of the CB docs as they are the ones who get indexed into ES
            // In V3 we need also to get the existing documents for deleted items as they will have to be re-indexes with a new parent document
            // representing the new transaction
            var existingAffectedAndDeletedPrograms = crudOperations.AffectedItems.Concat(crudOperations.ItemsToDelete);
            if (existingAffectedAndDeletedPrograms.Any())
            {
                var documentIds = _epgBL.GetEpgsCBKeys(_bulkUpload.GroupId, existingAffectedAndDeletedPrograms.Select(x => (long)x.EpgId), _languagesInfo.Languages.Values, false);
                var epgCbList = EpgDal.GetEpgCBList(documentIds);
                foreach (var existingProgram in existingAffectedAndDeletedPrograms)
                {
                    existingProgram.EpgCbObjects = epgCbList.Where(x => x.EpgID == existingProgram.EpgId).ToList();
                    existingProgram.EpgCbObjects.ForEach(e => e.StartDate = existingProgram.StartDate);
                    existingProgram.EpgCbObjects.ForEach(e => e.EndDate = existingProgram.EndDate);
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

            Utils.ExtractSuppressedValue(_catalogManagerAdapter.GetCatalogGroupCache(_partnerId), epgItem);

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
                _logger.LogInformation($"Program with external Id:[{epgItem.EpgIdentifier}] has no images to upload");
                return;
            }

            epgItem.pictures = epgItem.pictures ?? new List<EpgPicture>();

            foreach (var icon in icons)
            {
                var epgPicture = new EpgPicture();
                var imgUrl = icon.src;
                long ratioId = 0;
                long imageTypeId = 0;


                if (_isOpc)
                {
                    if (_groupRatioNamesToImageTypes.TryGetValue(icon.ratio, out var imgType))
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
                    ratioId = long.Parse(_nonOpcGroupRatios.FirstOrDefault(r => r.Value == icon.ratio).Key);
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


        private async Task ProduceUpdateBulkUpload(int partnerId,
            long bulkUploadId,
            List<BulkUploadProgramAssetResult> resultsToUpdate = null,
            CRUDOperations<EpgProgramBulkUploadObject> crudOps = null,
            Status[] errors = null,
            bool shouldSetAsFatal = false)
        {
            var msg = new UpdateBulkUpload
            {
                BulkUploadId = bulkUploadId,
                PartnerId = partnerId,
            };

            if (resultsToUpdate?.Any() == true) { await _bulkUploadRepository.InsertBulkUploadResults(partnerId, resultsToUpdate); }

            if (crudOps != null) { await _bulkUploadRepository.InsertCrudOperations(partnerId, bulkUploadId, crudOps); }

            if (errors?.Any() == true) { await _bulkUploadRepository.InsertErrors(partnerId, _bulkUploadId, errors); }

            msg.SetStatusToFatal = shouldSetAsFatal;

            await _kafkaUpdateBulkUploadProducer.ProduceAsync(UpdateBulkUpload.GetTopic(), msg.GetPartitioningKey(), msg);
        }

        private KMonitor CreateKMonitor(string name, params string[] args)
        {
            var argsStr = args?.Any() == true ? $"-{string.Join("-", args)}" : "";
            return new KMonitor(_kmonEvt, _partnerIdStr, $"ingest-profiler-{name}-{_partnerIdStr}-{_bulkUploadId}-{_linearChannelId}{argsStr}");
        }

        private void SetProgramsWithMissedEpgIds(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            _logger.LogDebug($"CalculateSimulatedFinalStateAfterIngest > adding EpgIds to new programs");
            if (crudOperations.ItemsToAdd.Any())
            {
                var missedIdsItems = crudOperations.ItemsToAdd.Where(x => x.EpgId == 0).ToArray();
                if (missedIdsItems.Length == 0)
                {
                    return;
                }

                var newIds = _epgBL.GetNewEpgIds(missedIdsItems.Length).ToArray();
                for (int i = 0; i < missedIdsItems.Length; i++)
                {
                    missedIdsItems[i].EpgId = newIds[i];
                }
            }
        }
    }
}
