using ApiLogic;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.EventBus;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using Core.Profiles;
using CouchbaseManager;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using EpgBL;
using EventBus.Abstraction;
using EventBus.RabbitMQ;
using GroupsCacheManager;
using IngestHandler.Common;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using TVinciShared;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly ElasticSearchApi _ElasticSearchClient = null;
        private readonly CouchbaseManager.CouchbaseManager _CouchbaseManager = null;

        private TvinciEpgBL _EpgBL;
        private BulkUploadIngestEvent _EventData;
        private BulkUpload _BulkUploadObject;
        private BulkUploadIngestJobData _BulkUploadJobData;
        private IngestProfile _IngestProfile;
        private IDictionary<string, LanguageObj> _Languages;
        // key = ratioId , value = ration value
        private Dictionary<string, string> _NonOpcGroupRatios;
        private Dictionary<string, ImageType> _GroupRatioNamesToImageTypes;
        private LanguageObj _DefaultLanguage;
        private bool _IsOpc;

        /// <summary>
        /// Programs By ExternalId wrapped in a dictionary by ChannelId 
        /// int - ChannelId
        /// Dictionary<string,ProgramAssetResult> - results by externalId
        /// </summary>
        private Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> _ResultsDictionary;
        private DateTime _MinStartDate;
        private DateTime _MaxEndDate;
        private Dictionary<string, EpgCB> _AutoFillEpgsCB;

        public BulkUploadIngestHandler()
        {
            _ElasticSearchClient = new ElasticSearchApi();
            _CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);
        }

        public async Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                _Logger.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                _EpgBL = new TvinciEpgBL(serviceEvent.GroupId);
                _EventData = serviceEvent;
                _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(serviceEvent.GroupId, serviceEvent.BulkUploadId);
                BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Processing);

                _BulkUploadJobData = _BulkUploadObject.JobData as BulkUploadIngestJobData;
                _IngestProfile = GetIngestProfile();
                _Languages = GetGroupLanguages(out _DefaultLanguage);
                _NonOpcGroupRatios = EpgDal.Get_PicsEpgRatios();
                _GroupRatioNamesToImageTypes = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(serviceEvent.GroupId);
                _IsOpc = GroupSettingsManager.IsOpc(serviceEvent.GroupId);

                ValidateServiceEvent();

                _ResultsDictionary = GetProgramAssetResults();

                var programsToIngest = _ResultsDictionary.Values.SelectMany(r => r.Values).Select(r => r.Object).Cast<EpgProgramBulkUploadObject>().ToList();

                _MinStartDate = programsToIngest.Min(p => p.StartDate);
                _MaxEndDate = programsToIngest.Max(p => p.EndDate);
                var currentPrograms = GetCurrentProgramsByDate(_MinStartDate, _MaxEndDate);

                var programsToIngestPerChannel = programsToIngest.GroupBy(p => p.ChannelId).ToDictionary(k => k.Key, v => v.ToList());
                var currentProgramsByChannel = currentPrograms.GroupBy(p => p.ChannelId).ToDictionary(k => k.Key, v => v.ToList());
                var overallCrudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();

                foreach (var channelId in programsToIngestPerChannel.Keys)
                {
                    var programsToIngestOfChannel = programsToIngestPerChannel[channelId];
                    if (!currentProgramsByChannel.TryGetValue(channelId, out var currentProgramsOfChannel))
                    {
                        currentProgramsOfChannel = new List<EpgProgramBulkUploadObject>();
                    }

                    var crudOperationsForChannel = HandleIngestForChannel(currentProgramsOfChannel, programsToIngestOfChannel, out var isValid);
                    if (!isValid) { return; }

                    overallCrudOperations.AddRange(crudOperationsForChannel);
                }

                var finalEpgSchedule = overallCrudOperations.ItemsToAdd
                        .Concat(overallCrudOperations.ItemsToUpdate)
                        .Concat(overallCrudOperations.AffectedItems)
                        .ToList();

                await UploadEpgImages(finalEpgSchedule);

                var isCloneSuccess = CloneExistingIndex();
                if (!isCloneSuccess)
                {
                    _Logger.Error($"Failed cloning Index, bulkId:[{_BulkUploadObject.Id}], groupId:[{_BulkUploadObject.GroupId}]");
                    UpdateBulkUploadObjectStatusAndResults(BulkUploadJobStatus.Failed);
                    return;
                }

                if (overallCrudOperations.AffectedItems.Any())
                {
                    BulkUploadManager.UpdateOrAddBulkUploadAffectedObjects(_BulkUploadObject.Id, overallCrudOperations.AffectedItems);
                }

                await BulkUploadMethods.UpdateCouchbase(finalEpgSchedule, serviceEvent.GroupId);

                var updater = new EpgElasticUpdater(serviceEvent.GroupId, serviceEvent.BulkUploadId, serviceEvent.DateOfProgramsToIngest, _Languages);
                updater.Update(finalEpgSchedule, overallCrudOperations.ItemsToDelete);

                var isErrorInEpg = _ResultsDictionary.Values.SelectMany(r => r.Values).Any(item => item.Status == BulkUploadResultStatus.Error);
                if (isErrorInEpg)
                {
                    _Logger.Error($"Failed Ingest due to errors in some of the results, bulkId:[{_BulkUploadObject.Id}], groupId:[{_BulkUploadObject.GroupId}]");
                    UpdateBulkUploadObjectStatusAndResults(BulkUploadJobStatus.Failed);
                    return;
                }


                // in case there are no errors update the results and set status according to them before sending for validation
                UpdateBulkUploadObjectStatusAndResults();

                // publish using EventBus to a new consumer with a new event ValidateIngest
                var publisher = EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
                var bulkUploadIngestValidationEvent = new BulkUploadIngestValidationEvent
                {
                    BulkUploadId = serviceEvent.BulkUploadId,
                    GroupId = serviceEvent.GroupId,
                    RequestId = KLogger.GetRequestId(),
                    DateOfProgramsToIngest = serviceEvent.DateOfProgramsToIngest,
                    EPGs = finalEpgSchedule,
                    Languages = _Languages,
                    Results = _ResultsDictionary
                };

                publisher.Publish(bulkUploadIngestValidationEvent);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                try
                {
                    _Logger.Debug($"Trying to set fatal status on bulk");
                    _BulkUploadObject.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest, {ex.Message}");
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Fatal);
                }
                catch (Exception innerEx)
                {
                    _Logger.Error($"An Exception occurred when trying to set FATAL status on bulkUpload. requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", innerEx);
                    throw;
                }
                throw;
            }

            return;
        }

        private CRUDOperations<EpgProgramBulkUploadObject> HandleIngestForChannel(List<EpgProgramBulkUploadObject> currentPrograms, List<EpgProgramBulkUploadObject> programsToIngestOfChannel, out bool isValid)
        {
            isValid = true;
            var overlapsInIngestSource = GetOverlappingPrograms(programsToIngestOfChannel);
            ValidateSourceInputOverlaps(overlapsInIngestSource);

            var crudOperationsForChannel = CalculateCRUDOperations(currentPrograms, programsToIngestOfChannel);
            SetProgramsWithEpgIds(crudOperationsForChannel);
            var isOverlapValid = CalculateOverlapsByPolicy(crudOperationsForChannel);
            var isGapValid = CalculateGapsByPolicy(crudOperationsForChannel);

            if (!isOverlapValid || !isGapValid)
            {
                _Logger.Debug($"Overlaps or gaps are not valid by ingest profile, bulkId:[{_BulkUploadObject.Id}], groupId:[{_BulkUploadObject.GroupId}]");
                UpdateBulkUploadObjectStatusAndResults(BulkUploadJobStatus.Failed);
                isValid = false;
                return crudOperationsForChannel;
            }

            AddEpgCBObjects(_ResultsDictionary);
            return crudOperationsForChannel;
        }

        private void UpdateBulkUploadObjectStatusAndResults(BulkUploadJobStatus? statusToSet = null)
        {
            var resultsToUpdate = _ResultsDictionary.Values.SelectMany(r => r.Values).ToList();
            BulkUploadManager.UpdateBulkUploadResults(resultsToUpdate, out var jobStatusByResultStatus);
            var jobStatus = statusToSet?? jobStatusByResultStatus;
            
            _Logger.Debug($"UpdateBulkUploadObjectStatusAndResults > updated results, calculated status by results: [{jobStatusByResultStatus}], requested status to set:[{statusToSet}], setting status:[{jobStatus}]");
            BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, jobStatus);
        }

        private bool CalculateGapsByPolicy(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var isValid = true;

            // If we can keep holes in EPG just avoid any calculation
            if (_IngestProfile.DefaultAutoFillPolicy == eIngestProfileAutofillPolicy.KeepHoles) { return true; }

            var gaps = new List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>>();
            var autofillEpgs = new List<EpgProgramBulkUploadObject>();
            var newEpgSchedule = crudOperations.ItemsToAdd
                .Concat(crudOperations.ItemsToUpdate)
                .Concat(crudOperations.AffectedItems)
                .Concat(crudOperations.RemainingItems)
                .OrderBy(i => i.StartDate)
                .ToList();

            for (int i = 0; i < newEpgSchedule.Count - 1; i++)
            {
                var prog = newEpgSchedule[i];
                var nextProg = newEpgSchedule[i + 1];

                if (prog.EndDate < nextProg.StartDate)
                {
                    gaps.Add(Tuple.Create(prog, nextProg));
                }
            }

            switch (_IngestProfile.DefaultAutoFillPolicy)
            {
                case eIngestProfileAutofillPolicy.Reject:
                    if (gaps.Any())
                    {
                        gaps.ForEach(gappedProgs =>
                        {
                            var gapStartTime = gappedProgs.Item1.EndDate;
                            var gapEndTime = gappedProgs.Item2.StartDate;
                            var errorMessage = $"Program {gappedProgs.Item1.EpgExternalId} end: {gapStartTime} creates a gap with another program {gappedProgs.Item2.EpgExternalId} start: {gapEndTime}";
                            // Using try add error on both items because we are not sure which program is from source and which is existing
                            var isUpdateSuccessItem1 = TryAddError(gappedProgs.Item1.ChannelId, gappedProgs.Item1.EpgExternalId, eResponseStatus.EPGSProgramDatesError, errorMessage);
                            var isUpdateSuccessItem2 = TryAddError(gappedProgs.Item2.ChannelId, gappedProgs.Item2.EpgExternalId, eResponseStatus.EPGSProgramDatesError, errorMessage);

                            // this means the gap was created between 2 programs in current epg schedule and maybe an update caused the gap
                            // so we are gonna search for the culprit
                            if (!isUpdateSuccessItem1 && !isUpdateSuccessItem2)
                            {
                                var updatedProgramsCausingTheGap = crudOperations.ItemsToUpdate.Where(p => p.StartDate >= gapStartTime && p.EndDate >= gapEndTime).ToList();
                                if (updatedProgramsCausingTheGap.Any())
                                {
                                    updatedProgramsCausingTheGap.ForEach(p =>
                                    {
                                        errorMessage = $"Program {p.EpgExternalId} update, is causing a gap between program {gappedProgs.Item1.EpgExternalId} end: {gapStartTime} creates and program {gappedProgs.Item2.EpgExternalId} start: {gapEndTime}";

                                        _ResultsDictionary[p.ChannelId][p.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, errorMessage);
                                    });
                                }
                                else
                                {
                                    //This means we found an unxplainable gap and we cannot identify what was the root cause for it, so we put a general error;
                                    _BulkUploadObject.AddError(eResponseStatus.EPGSProgramDatesError, errorMessage);
                                }
                            }
                        });
                        isValid = false;
                    }
                    break;
                case eIngestProfileAutofillPolicy.Autofill:
                    gaps.ForEach(gappedProgs =>
                    {
                        var warnMessage = $"Autofilling gap in between {gappedProgs.Item1.EpgExternalId} end: {gappedProgs.Item1.EndDate} and{gappedProgs.Item2.EpgExternalId} start: {gappedProgs.Item2.StartDate}";
                        // Using try add warnning on both items because we are not sure which program is from source and which is existing
                        TryAddWarnning(gappedProgs.Item1.ChannelId, gappedProgs.Item1.EpgExternalId, eResponseStatus.EPGSProgramDatesError, warnMessage);
                        TryAddWarnning(gappedProgs.Item2.ChannelId, gappedProgs.Item2.EpgExternalId, eResponseStatus.EPGSProgramDatesError, warnMessage);
                        var autofillProgram = GetDefaultAutoFillProgram(gappedProgs.Item1.EndDate, gappedProgs.Item2.StartDate, gappedProgs.Item1.ChannelId, gappedProgs.Item1.ChannelExternalId, gappedProgs.Item1.LinearMediaId);
                        autofillEpgs.Add(autofillProgram);
                    });
                    break;
            }

            // Add new epgIds to autofill objects
            if (autofillEpgs.Any())
            {
                var ids = _EpgBL.GetNewEpgIds(autofillEpgs.Count).ToList();
                for (int i = 0; i < autofillEpgs.Count; i++)
                {
                    autofillEpgs[i].EpgId = ids[i];
                }
            }

            return isValid;
        }

        private void ValidateSourceInputOverlaps(List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> overlapsInIngestSource)
        {
            if (overlapsInIngestSource.Any())
            {
                overlapsInIngestSource.ForEach(p =>
                {
                    var errorMessage = $"Program to ingets {p.Item1.EpgExternalId} is overlapping another programs to ingest {p.Item2.EpgExternalId}";
                    _ResultsDictionary[p.Item1.ChannelId][p.Item1.EpgExternalId].AddError(eResponseStatus.Error, errorMessage);
                    _ResultsDictionary[p.Item2.ChannelId][p.Item2.EpgExternalId].AddError(eResponseStatus.Error, errorMessage);
                });
            }
        }

        private bool CalculateOverlapsByPolicy(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var isValid = true;
            var exsitingNewEpg = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate).Concat(crudOperations.RemainingItems).ToList();
            var allNew = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate).ToList();
            var overlaps = GetOverlappingPrograms(allNew,exsitingNewEpg);

            foreach (var overlappingProgs in overlaps)
            {
                var progToIngest = overlappingProgs.Item1;
                var existingProg = overlappingProgs.Item2;

                var msg = $"Program [{progToIngest.EpgExternalId}] overlapping [{existingProg.EpgExternalId}";

                switch (_IngestProfile.DefaultOverlapPolicy)
                {
                    case eIngestProfileOverlapPolicy.Reject:
                        msg += $", policy is set to Reject, rejecting input on overlapping items";
                        _ResultsDictionary[progToIngest.ChannelId][progToIngest.EpgExternalId].AddError(eResponseStatus.Error, msg);
                        isValid = false;
                        break;
                    case eIngestProfileOverlapPolicy.CutSource:
                        if (progToIngest.EndDate > existingProg.StartDate)
                        {
                            msg += $", policy is set to CutSource, cutting current programs end date from [{progToIngest.EndDate}], to [{existingProg.StartDate}]";
                            progToIngest.EndDate = existingProg.StartDate;
                        }
                        else
                        {
                            msg += $", policy is set to CutSource, cutting current programs start date from [{progToIngest.StartDate}], to [{existingProg.EndDate}]";
                            progToIngest.StartDate = existingProg.EndDate;
                        }

                        _ResultsDictionary[progToIngest.ChannelId][progToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        break;
                    case eIngestProfileOverlapPolicy.CutTarget:
                        if (existingProg.EndDate > progToIngest.StartDate)
                        {
                            msg += $", policy is set to CutTarget, cutting existing programs end date from [{existingProg.EndDate}], to [{progToIngest.StartDate}]";
                            existingProg.EndDate = progToIngest.StartDate;
                        }
                        else
                        {
                            msg += $", policy is set to CutTarget, cutting existing programs start date from [{existingProg.StartDate}], to [{progToIngest.EndDate}]";
                            existingProg.StartDate = progToIngest.EndDate;
                        }

                        crudOperations.AffectedItems.Add(existingProg);
                        crudOperations.RemainingItems.Remove(existingProg);
                        _ResultsDictionary[progToIngest.ChannelId][progToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        break;
                }
            }

            return isValid;
        }

        private IDictionary<string, LanguageObj> GetGroupLanguages(out LanguageObj defaultLanguage)
        {
            var languages = GroupLanguageManager.GetGroupLanguages(_EventData.GroupId);
            defaultLanguage = languages.FirstOrDefault(l => l.IsDefault);
            if (defaultLanguage == null) { throw new Exception($"No main language defined for group:[{_EventData.GroupId}], ingest failed"); }
            return languages.ToDictionary(l => l.Code);
        }

        private async Task UploadEpgImages(List<EpgProgramBulkUploadObject> programsToIngest)
        {
            var pics = programsToIngest.SelectMany(p => p.EpgCbObjects).SelectMany(p => p.pictures);
            var results = await EpgImageManager.UploadEPGPictures(_EventData.GroupId, pics);
        }

        private void AddEpgCBObjects(Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> results)
        {
            _Logger.Debug($"Generating EpgCB translation object for every bulk request, with languages:[{string.Join(",", _Languages.Keys)}]");

            foreach (var item in results.Values)
            {
                foreach (var progResult in item.Values)
                {
                    var prog = progResult.Object as EpgProgramBulkUploadObject;
                    prog.EpgCbObjects = new List<EpgCB>();
                    if (prog.IsAutoFill)
                    {
                        var epgItems = GetAutoFillEpgCBDocuments(prog);
                        prog.EpgCbObjects.AddRange(epgItems.Values);
                    }
                    else
                    {
                        foreach (var lang in _Languages.Values)
                        {
                            var epgItem = GenerateEpgCBObject(lang.Code, _DefaultLanguage.Code, prog, progResult);
                            prog.EpgCbObjects.Add(epgItem);
                        }
                    }
                }
            }
        }

        private EpgCB GenerateEpgCBObject(string langCode, string defaultLangCode, EpgProgramBulkUploadObject prog, BulkUploadProgramAssetResult bulkUploadResultItem)
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

            epgItem.Metas = parsedProg.ParseMetas(langCode, defaultLangCode, bulkUploadResultItem);
            epgItem.Tags = parsedProg.ParseTags(langCode, defaultLangCode, bulkUploadResultItem);

            PrepareEpgItemImages(parsedProg.icon, epgItem, bulkUploadResultItem);

            return epgItem;
        }

        private static string GetEpgCBDocumentId(ulong epgId, long bulkUploadId, string langCode)
        {
            return $"epg_{bulkUploadId}_{langCode}_{epgId}";
        }

        private void PrepareEpgItemImages(icon[] icons, EpgCB epgItem, BulkUploadProgramAssetResult bulkUploadResultItem)
        {
            if (icons?.Any() != true)
            {
                _Logger.Info($"Program with external Id:[{epgItem.EpgIdentifier}] has no images to upload");
                return;
            }

            epgItem.pictures = epgItem.pictures ?? new List<EpgPicture>();
            var result = new List<EpgPicture>();
            foreach (var icon in icons)
            {
                var epgPicture = new EpgPicture();
                var imgUrl = icon.src;
                long ratioId = 0;
                long imageTypeId = 0;


                if (_IsOpc)
                {
                    if (_GroupRatioNamesToImageTypes.TryGetValue(icon.ratio, out var imgType))
                    {
                        imageTypeId = imgType.Id;
                        ratioId = imgType.RatioId.Value;
                    }

                }
                else
                {
                    ratioId = long.Parse(_NonOpcGroupRatios.FirstOrDefault(r => r.Value == icon.ratio).Key);
                }

                epgPicture.Url = imgUrl;
                epgPicture.PicID = -1;
                epgPicture.Ratio = icon.ratio;
                epgPicture.RatioId = (int)ratioId;
                epgPicture.ImageTypeId = imageTypeId;
                epgPicture.ProgramName = epgItem.Name;
                epgPicture.ChannelId = epgItem.ChannelID;
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

        private void ValidateServiceEvent()
        {
            _Logger.Debug($"ValidateServiceEvent: _EventData.ProgramsToIngest.Count:[{_EventData?.ProgramsToIngest?.Count}]");
            if (_EventData.ProgramsToIngest?.Any() != true)
            {
                throw new Exception($"Received bulk upload ingest event with null or empty programs to insert. group id = {_EventData.GroupId} id = {_EventData.BulkUploadId}");
            }

            if (_EventData.ProgramsToIngest.Count() == 0)
            {
                _Logger.Warn($"Received bulk upload ingest event with 0 programs to insert. group id ={_EventData.GroupId} id = {_EventData.BulkUploadId}");
            }

            var targetIndex = IndexManager.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);
            if (_ElasticSearchClient.IndexExists(targetIndex))
            {
                _Logger.Warn($"already found index:[{targetIndex}], removing it before ingest starts");
                _ElasticSearchClient.DeleteIndices(new List<string> { targetIndex });
            }
        }

        private Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>> GetProgramAssetResults()
        {
            var res = new Dictionary<int, Dictionary<string, BulkUploadProgramAssetResult>>();

            _Logger.Debug($"Creating bulk results dictionary for:[{_EventData.ProgramsToIngest.Count}] programs to ingest");

            foreach (var bulkUploadProgram in _BulkUploadObject.Results.Cast<BulkUploadProgramAssetResult>())
            {
                var program = _EventData.ProgramsToIngest.FirstOrDefault(p => p.EpgExternalId == bulkUploadProgram.ProgramExternalId
                                                                    && p.LinearMediaId == bulkUploadProgram.LiveAssetId);

                if (program != null)
                {
                    if (!res.ContainsKey(program.ChannelId))
                    {
                        res.Add(program.ChannelId, new Dictionary<string, BulkUploadProgramAssetResult>());
                    }

                    bulkUploadProgram.Object = program;
                    res[program.ChannelId].Add(bulkUploadProgram.ProgramExternalId, bulkUploadProgram);
                }
            }

            return res;
        }

        private List<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(DateTime minStartDate, DateTime maxEndDate)
        {
            _Logger.Debug($"GetCurrentProgramsByDate > minStartDate:[{minStartDate}], maxEndDate:[{maxEndDate}]");
            var result = new List<EpgProgramBulkUploadObject>();
            string index = _EpgBL.GetProgramIndexAlias();

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_ElasticSearchClient.IndexExists(index))
            {
                _Logger.Debug($"GetCurrentProgramsByDate > index alias:[{index}] does not exsits, assuming no current programs");
                return result;
            }

            _Logger.Debug($"GetCurrentProgramsByDate > index alias:[{index}] found, searching current programs, minStartDate:[{minStartDate}], maxEndDate:[{maxEndDate}]");

            var query = new FilteredQuery(true);

            // Program end date > minimum start date
            // program start date < maximum end date
            var minimumRange = new ESRange(false, "start_date", eRangeComp.GTE, minStartDate.ToString(ESUtils.ES_DATEONLY_FORMAT) + "000000");
            var maximumRange = new ESRange(false, "end_date", eRangeComp.LTE, maxEndDate.ToString(ESUtils.ES_DATEONLY_FORMAT) + "235959");


            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(minimumRange);
            filterCompositeType.AddChild(maximumRange);

            query.Filter = new QueryFilter()
            {
                FilterSettings = filterCompositeType
            };

            query.ReturnFields.Clear();
            query.AddReturnField("_index");
            query.AddReturnField("document_id");
            query.AddReturnField("epg_id");
            query.AddReturnField("start_date");
            query.AddReturnField("end_date");
            query.AddReturnField("epg_identifier");
            query.AddReturnField("is_auto_fill");
            query.AddReturnField("epg_channel_id");

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _ElasticSearchClient.Search(index, EpgElasticUpdater.DEFAULT_INDEX_MAPPING_TYPE, ref searchQuery);


            List<string> documentIds = new List<string>();

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                var json = JObject.Parse(searchResult);

                var hits = (json["hits"]["hits"] as JArray);

                foreach (var hit in hits)
                {
                    var hitFields = hit["fields"];
                    var epgItem = new EpgProgramBulkUploadObject();
                    epgItem.EpgExternalId = ESUtils.ExtractValueFromToken<string>(hitFields, "epg_identifier");
                    epgItem.StartDate = ESUtils.ExtractDateFromToken(hit["fields"], "start_date");
                    epgItem.EndDate = ESUtils.ExtractDateFromToken(hit["fields"], "end_date");
                    epgItem.EpgId = ESUtils.ExtractValueFromToken<ulong>(hitFields, "epg_id");
                    epgItem.IsAutoFill = ESUtils.ExtractValueFromToken<bool>(hitFields, "is_auto_fill");
                    epgItem.ChannelId = ESUtils.ExtractValueFromToken<int>(hitFields, "epg_channel_id");

                    result.Add(epgItem);
                }
            }

            return result;
        }

        private CRUDOperations<EpgProgramBulkUploadObject> CalculateCRUDOperations(IList<EpgProgramBulkUploadObject> currentPrograms, IList<EpgProgramBulkUploadObject> programsToIngest)
        {
            // TODO: query elastic according to all externalIds of programs to ingest to get the exsiting programs. 
            // this is in case the exsisting program should be updated but actually moved from a different day or is out of the ingest time range
            // also there is an issue with the program beeing updated should be deleted \ moved from existing date .. this is a whole other can of worms :\
            // for now we will handle this by passing currentPrograms for the whole day
            // note we will add a filter for epg.StartDate >= _MinStartDate && epg.EndDate <= _MaxEndDate  before returning programs to dlete 


            _Logger.Debug($"CalculateCRUDOperations > currentPrograms.count:[{currentPrograms.Count}] programsToIngest.count:[{programsToIngest.Count}]");
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();

            crudOperations.ItemsToDelete = currentPrograms.Where(epg => epg.StartDate >= _MinStartDate && epg.EndDate <= _MaxEndDate && epg.IsAutoFill).ToList();

            var currentProgramsDictionary = currentPrograms.Where(epg => !epg.IsAutoFill).ToDictionary(epg => epg.EpgExternalId);
            _Logger.Debug($"CalculateCRUDOperations > currentProgramsDictionary.Count:[{currentProgramsDictionary.Count}], programsToIngest:[{programsToIngest.Count}]");

            // we cannot use the programs to ingest as a dictioanry becuse there are multiple translation with same external id
            // var programsToIngestDictionary = programsToIngest.ToDictionary(epg => epg.EpgIdentifier);
            foreach (var programToIngest in programsToIngest)
            {
                // if a program exists both on newly ingested epgs and in index - it's an update
                if (currentProgramsDictionary.ContainsKey(programToIngest.EpgExternalId))
                {
                    // update the epg id of the ingested programs with their existing epg id from CB
                    var idToUpdate = currentProgramsDictionary[programToIngest.EpgExternalId].EpgId;
                    programToIngest.EpgId = idToUpdate;
                    //programToIngest.EpgCbObjects.ForEach(p => p.EpgID = idToUpdate);
                    crudOperations.ItemsToUpdate.Add(programToIngest);
                }
                else
                {
                    // if it exists only on newly ingested epgs and not in index, it's a program to add
                    crudOperations.ItemsToAdd.Add(programToIngest);
                }

                // Remove programs that marked to add or update so that all that is left will be to delete
                currentProgramsDictionary.Remove(programToIngest.EpgExternalId);
            }

            // all update or add programs were removed form list so we left with items to delete
            crudOperations.ItemsToDelete.AddRange(currentProgramsDictionary.Values.Where(epg => epg.StartDate >= _MinStartDate && epg.EndDate <= _MaxEndDate).ToList());
            crudOperations.RemainingItems.AddRange(currentPrograms.Except(crudOperations.ItemsToDelete).Except(crudOperations.ItemsToUpdate));
            _Logger.Debug($"CalculateCRUDOperations > add:[{crudOperations.ItemsToAdd.Count}], update:[{crudOperations.ItemsToUpdate.Count}], delete:[{crudOperations.ItemsToDelete.Count}], remaining items in day:[{crudOperations.RemainingItems.Count}]");

            return crudOperations;
        }

        private void SetProgramsWithEpgIds(CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            _Logger.Debug($"CalculateSimulatedFinalStateAfterIngest > adding EpgIds to new programs");
            if (crudOperations.ItemsToAdd.Any())
            {
                var newIds = _EpgBL.GetNewEpgIds(crudOperations.ItemsToAdd.Count).ToList();
                for (int i = 0; i < crudOperations.ItemsToAdd.Count; i++)
                {
                    var idToSet = newIds[i];
                    crudOperations.ItemsToAdd[i].EpgId = idToSet;
                }
            }
        }

        private EpgProgramBulkUploadObject GetDefaultAutoFillProgram(DateTime start, DateTime end, int channelId, string channelExternalId, long leniarMediaId)
        {
            //EpgCB autoFillEpgCB = ObjectCopier.Clone(autoFillProgram);
            //var epgExternalId = Guid.NewGuid().ToString();



            return new EpgProgramBulkUploadObject()
            {
                StartDate = start,
                EndDate = end,
                IsAutoFill = true,
                ChannelId = channelId,
                EpgExternalId = Guid.NewGuid().ToString(),
                ParentGroupId = _BulkUploadObject.GroupId,
                GroupId = _BulkUploadObject.GroupId,
                ChannelExternalId = channelExternalId,
                LinearMediaId = leniarMediaId
                //EpgCbObjects = new List<EpgCB>() { autoFillEpgCB }
            };
        }

        /// <summary>
        /// real name: epg_203_20190422_123456
        /// alias: epg_203_20190422
        /// reindex epg_203_20190422 to epg_203_20190422_current_bulk_upload_id
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="bulkUploadId"></param>
        /// <param name="dateOfIngest"></param>
        private bool CloneExistingIndex()
        {
            var result = true;
            string source = IndexManager.GetIngestCurrentProgramsAliasName(_EventData.GroupId, _EventData.DateOfProgramsToIngest);
            string destination = IndexManager.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);

            if (_ElasticSearchClient.IndexExists(source))
            {
                result &= CloneAndReindexData(source, destination);
            }
            else
            {
                result &= BuildNewIndex(destination);
            }

            return result;
        }

        private bool CloneAndReindexData(string source, string destination)
        {

            var isCloneSuccess = _ElasticSearchClient.CloneIndexWithoutData(source, destination);
            var isReindexSuccess = false;
            if (isCloneSuccess)
            {
                isReindexSuccess = _ElasticSearchClient.Reindex(source, destination);
                if (!isReindexSuccess) { _Logger.ErrorFormat($"Reindex {source} to {destination} failure"); }
            }
            else
            {
                _Logger.ErrorFormat($"Reindex {source} to {destination} failure");
            }

            _Logger.Debug($"Clone and Reindex {source} to {destination} success");

            return isCloneSuccess && isReindexSuccess;
        }

        private IngestProfile GetIngestProfile()
        {
            var ingestProfile = IngestProfileManager.GetIngestProfileById(_EventData.GroupId, _BulkUploadJobData.IngestProfileId)?.Object;

            if (ingestProfile == null)
            {
                string message = $"Received bulk upload ingest event with invalid ingest profile.";
                _Logger.Error(message);
                throw new Exception(message);
            }

            return ingestProfile;
        }

        private bool BuildNewIndex(string newIndexName)
        {
            try
            {
                CatalogManager.TryGetCatalogGroupCacheFromCache(_EventData.GroupId, out var catalogGroupCache);
                var groupManager = new GroupManager();
                groupManager.RemoveGroup(_EventData.GroupId);
                var group = groupManager.GetGroup(_EventData.GroupId);
                _ = IndexManager.CreateNewEpgIndex(_EventData.GroupId, catalogGroupCache, group, _Languages.Values, _DefaultLanguage, newIndexName);
            }
            catch (Exception e)
            {
                _Logger.Error("Error while building new index. ", e);
                _BulkUploadObject.AddError(eResponseStatus.Error, "Error while building new index. ");
                return false;
            }

            return true;
        }

        private Dictionary<string, EpgCB> GetAutoFillEpgCBDocuments(EpgProgramBulkUploadObject prog)
        {
            var key = $"autofill_{_BulkUploadObject.GroupId}";
            _AutoFillEpgsCB = _AutoFillEpgsCB ?? _CouchbaseManager.Get<Dictionary<string, EpgCB>>(key, true);
            var autofillDocs = new Dictionary<string, EpgCB>();
            foreach (var doc in _AutoFillEpgsCB)
            {
                autofillDocs[doc.Key] = ObjectCopier.Clone(doc.Value);

                autofillDocs[doc.Key].EpgID = prog.EpgId;
                autofillDocs[doc.Key].DocumentId = GetEpgCBDocumentId(prog.EpgId, _BulkUploadObject.Id, doc.Key);
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
            }
            return _AutoFillEpgsCB;
        }



        private List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> GetOverlappingPrograms(List<EpgProgramBulkUploadObject> listOfPrograms)
        {
            return GetOverlappingPrograms(listOfPrograms, listOfPrograms);
        }


        /// <summary>
        /// Get list of overlapping programs between tow lists
        /// </summary>
        /// <param name="listOfPrograms">1st list</param>
        /// <param name="otherListOfPrograms">2nd list</param>
        /// <returns>List of overlapping pairs, item1 is from 1st list item2 is from 2nd</returns>
        private List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>> GetOverlappingPrograms(List<EpgProgramBulkUploadObject> listOfPrograms, List<EpgProgramBulkUploadObject> otherListOfPrograms)
        {
            var overlappingPairs = new List<Tuple<EpgProgramBulkUploadObject, EpgProgramBulkUploadObject>>();
            foreach (var prog in listOfPrograms)
            {
                var allOtherPrograms = otherListOfPrograms.Where(p => p.EpgExternalId != prog.EpgExternalId);
                foreach (var otherProg in allOtherPrograms)
                {
                    if (IsOverlappingPrograms(prog, otherProg))
                    {
                        overlappingPairs.Add(Tuple.Create(prog, otherProg));
                    }
                }
            }

            return overlappingPairs;
        }


        private static bool IsOverlappingPrograms(EpgProgramBulkUploadObject prog, EpgProgramBulkUploadObject otherProg)
        {
            // if prog starts befor other ends AND other start before the prog ends
            return prog.StartDate < otherProg.EndDate && otherProg.StartDate < prog.EndDate;
        }

        private bool TryAddError(int channelId, string epgExternalId, eResponseStatus status, string msg)
        {
            if (_ResultsDictionary.TryGetValue(channelId, out var resultsOfChannel))
            {
                if (resultsOfChannel.TryGetValue(epgExternalId, out var epgResultObject))
                {
                    epgResultObject.AddError(status, msg);
                    return true;
                }
            }

            return false;
        }

        private bool TryAddWarnning(int channelId, string epgExternalId, eResponseStatus status, string msg)
        {
            if (_ResultsDictionary.TryGetValue(channelId, out var resultsOfChannel))
            {
                if (resultsOfChannel.TryGetValue(epgExternalId, out var epgResultObject))
                {
                    epgResultObject.AddWarning((int)status, msg);
                    return true;
                }
            }

            return false;
        }
    }
}