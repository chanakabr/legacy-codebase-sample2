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

        public const string LOWERCASE_ANALYZER =
            "\"lowercase_analyzer\": {\"type\": \"custom\",\"tokenizer\": \"keyword\",\"filter\": [\"lowercase\"],\"char_filter\": [\"html_strip\"]}";

        public const string PHRASE_STARTS_WITH_FILTER =
            "\"edgengram_filter\": {\"type\":\"edgeNGram\",\"min_gram\":1,\"max_gram\":20,\"token_chars\":[\"letter\",\"digit\",\"punctuation\",\"symbol\"]}";

        public const string PHRASE_STARTS_WITH_ANALYZER =
            "\"phrase_starts_with_analyzer\": {\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\":[\"lowercase\",\"edgengram_filter\", \"icu_folding\",\"icu_normalizer\"]," +
            "\"char_filter\":[\"html_strip\"]}";

        public const string PHRASE_STARTS_WITH_SEARCH_ANALYZER =
            "\"phrase_starts_with_search_analyzer\": {\"type\":\"custom\",\"tokenizer\":\"keyword\",\"filter\":[\"lowercase\", \"icu_folding\",\"icu_normalizer\"]," +
            "\"char_filter\":[\"html_strip\"]}";

        protected const string ANALYZER_VERSION = "2";

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
        private Dictionary<string, BulkUploadProgramAssetResult> _Results;

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
                _GroupRatioNamesToImageTypes = EpgImageManager.GetImageTypesMapBySystemName(serviceEvent.GroupId);
                _IsOpc = GroupSettingsManager.IsOpc(serviceEvent.GroupId);

                ValidateServiceEvent();

                _Results = GetProgramAssetResults();
                AddEpgCBObjects(_Results);

                var programsToIngest = _Results.Values.Select(r => r.Object).Cast<EpgProgramBulkUploadObject>().ToList();
                await UploadEpgImages(programsToIngest);
                var minStartDate = programsToIngest.Min(p => p.StartDate);
                var maxEndDate = programsToIngest.Max(p => p.EndDate);
                var currentPrograms = GetCurrentProgramsByDate(minStartDate, maxEndDate);

                var crudOperations = CalculateCRUDOperations(currentPrograms, programsToIngest);

                var edgeProgramsToUpdate = CalculateRequiredUpdatesToEdgesDueToOverlap(currentPrograms, crudOperations);

                HandleOverlapsAndGaps(crudOperations, _Results);

                var finalEpgState = CalculateSimulatedFinalStateAfterIngest(crudOperations.ItemsToAdd, crudOperations.ItemsToUpdate);

                await BulkUploadMethods.UpdateCouchbase(finalEpgState, serviceEvent.GroupId);

                CloneExistingIndex();

                var updater = new UpdateClonedIndex(serviceEvent.GroupId, serviceEvent.BulkUploadId, serviceEvent.DateOfProgramsToIngest, _Languages);
                updater.Update(finalEpgState, crudOperations.ItemsToDelete);

                // publish using EventBus to a new consumer with a new event ValidateIngest
                var publisher = EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();

                var bulkUploadIngestValidationEvent = new BulkUploadIngestValidationEvent
                {
                    BulkUploadId = serviceEvent.BulkUploadId,
                    GroupId = serviceEvent.GroupId,
                    RequestId = KLogger.GetRequestId(),
                    DateOfProgramsToIngest = serviceEvent.DateOfProgramsToIngest,
                    EPGs = finalEpgState,
                    EdgeProgramsToUpdate = edgeProgramsToUpdate,
                    Languages = _Languages,
                    Results = _Results
                };

                publisher.Publish(bulkUploadIngestValidationEvent);
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                try
                {
                    _Logger.Debug($"Trying to set fatal status on bulk");
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Failed);
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

        private async Task UploadEpgImages(List<EpgProgramBulkUploadObject> programsToIngest)
        {
            var pics = programsToIngest.SelectMany(p => p.EpgCbObjects).SelectMany(p => p.pictures);
            var results = await EpgImageManager.UploadEPGPictures(_EventData.GroupId, pics);
        }

        private void AddEpgCBObjects(Dictionary<string, BulkUploadProgramAssetResult> results)
        {
            _Logger.Debug($"Generating EpgCB translation object for every bulk request, with languages:[{string.Join(",", _Languages.Keys)}]");
            foreach (var progResult in results.Values)
            {
                var programBulkUploadObject = progResult.Object as EpgProgramBulkUploadObject;
                programBulkUploadObject.EpgCbObjects = new List<EpgCB>();
                foreach (var lang in _Languages.Values)
                {
                    var epgItem = GetEpgCBObject(lang.Code, _DefaultLanguage.Code, programBulkUploadObject, progResult);
                    programBulkUploadObject.EpgCbObjects.Add(epgItem);
                }
            }
        }

        private IDictionary<string, LanguageObj> GetGroupLanguages(out LanguageObj defaultLanguage)
        {
            var languages = GroupLanguageManager.GetGroupLanguages(_EventData.GroupId);
            defaultLanguage = languages.FirstOrDefault(l => l.IsDefault);
            if (defaultLanguage == null) { throw new Exception($"No main language defined for group:[{_EventData.GroupId}], ingest failed"); }
            return languages.ToDictionary(l => l.Code);
        }

        private EpgCB GetEpgCBObject(string langCode, string defaultLangCode, EpgProgramBulkUploadObject prog, BulkUploadProgramAssetResult bulkUploadResultItem)
        {
            var epgItem = new EpgCB();
            var parsedProg = prog.ParsedProgramObject;

            epgItem.DocumentId = GetEpgCBDocumentId(parsedProg.external_id, langCode, bulkUploadResultItem.BulkUploadId);
            epgItem.Language = langCode;
            epgItem.ChannelID = prog.ChannelId;
            epgItem.LinearMediaId = prog.LinearMediaId;
            epgItem.GroupID = prog.GroupId;
            epgItem.ParentGroupID = prog.ParentGroupId;
            epgItem.EpgIdentifier = parsedProg.external_id;
            epgItem.StartDate = parsedProg.ParseStartDate(bulkUploadResultItem);
            epgItem.EndDate = parsedProg.ParseEndDate(bulkUploadResultItem);
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

        public static string GetEpgCBDocumentId(string external_id, string langCode, long bulkUploadId)
        {
            // TODO: validate if key need specific prefix
            return $"epg_{bulkUploadId}_{langCode}_{external_id}";
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

            var targetIndex = BulkUploadMethods.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);
            if (_ElasticSearchClient.IndexExists(targetIndex))
            {
                _Logger.Warn($"already found index:[{targetIndex}], removing it before ingest starts");
                _ElasticSearchClient.DeleteIndices(new List<string> { targetIndex });
            }
        }

        private Dictionary<string, BulkUploadProgramAssetResult> GetProgramAssetResults()
        {
            var programsToIngest = _EventData.ProgramsToIngest;
            _Logger.Debug($"Creating bulk results dictionary for:[{programsToIngest.Count}] programs to ingest");

            var programAssetResultsDictionary = _BulkUploadObject.Results
                .Cast<BulkUploadProgramAssetResult>()
                .ToDictionary(program => program.ProgramExternalId);

            foreach (var programToIngest in programsToIngest)
            {
                programAssetResultsDictionary[programToIngest.ParsedProgramObject.external_id].Object = programToIngest;
            }

            // Select only results that have objects asspciated with them, 
            // and ignore other results, theey should probably be handled by a different event
            programAssetResultsDictionary = programAssetResultsDictionary
                .Where(p => p.Value.Object != null)
                .ToDictionary(k => k.Key, v => v.Value);

            return programAssetResultsDictionary;
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
            var minimumRange = new ESRange(false, "end_date", eRangeComp.GTE, minStartDate.ToString(ESUtils.ES_DATE_FORMAT));
            var maximumRange = new ESRange(false, "start_date", eRangeComp.LTE, maxEndDate.ToString(ESUtils.ES_DATE_FORMAT));


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

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _ElasticSearchClient.Search(index, UpdateClonedIndex.DEFAULT_INDEX_MAPPING_TYPE, ref searchQuery);

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

                    result.Add(epgItem);
                }
            }

            return result;
        }

        private CRUDOperations<EpgProgramBulkUploadObject> CalculateCRUDOperations(IList<EpgProgramBulkUploadObject> currentPrograms, IList<EpgProgramBulkUploadObject> programsToIngest)
        {
            _Logger.Debug($"CalculateCRUDOperations > currentPrograms.count:[{currentPrograms.Count}] programsToIngest.count:[{programsToIngest.Count}]");
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();

            crudOperations.ItemsToDelete = currentPrograms.Where(epg => epg.IsAutoFill).ToList();

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
                    programToIngest.EpgCbObjects.ForEach(p => p.EpgID = idToUpdate);
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
            crudOperations.ItemsToDelete.AddRange(currentProgramsDictionary.Values.ToList());

            _Logger.Debug($"CalculateCRUDOperations > add:[{crudOperations.ItemsToAdd.Count}], update:[{crudOperations.ItemsToUpdate.Count}], delete:[{crudOperations.ItemsToDelete.Count}]");

            return crudOperations;
        }

        private List<EpgProgramBulkUploadObject> CalculateRequiredUpdatesToEdgesDueToOverlap(IList<EpgProgramBulkUploadObject> currentPrograms, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var result = new List<EpgProgramBulkUploadObject>();

            // If overlaps are allowed we have to check the edges of the give range for overlap and cut the source or the target
            if (_IngestProfile.DefaultOverlapPolicy != eIngestProfileOverlapPolicy.Reject)
            {
                _Logger.Debug($"CalculateCRUDOperations > _IngestProfile.DefaultOverlapPolicy:[{_IngestProfile.DefaultOverlapPolicy}], calculating required update to edge programs");

                result = CutSourceOrTargetOverlappingDates(currentPrograms.Except(crudOperations.ItemsToDelete),
                    crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate));

                _Logger.Debug($"CalculateCRUDOperations > after edge overlap calculations add:[{crudOperations.ItemsToAdd.Count}], update:[{crudOperations.ItemsToUpdate.Count}], delete:[{crudOperations.ItemsToDelete.Count}]");
            }

            return result;
        }

        private List<EpgProgramBulkUploadObject> CutSourceOrTargetOverlappingDates(IEnumerable<EpgProgramBulkUploadObject> currentPrograms, IEnumerable<EpgProgramBulkUploadObject> programsToIngest)
        {
            var result = new List<EpgProgramBulkUploadObject>();
            var orderedCurrentPrograms = currentPrograms.OrderBy(p => p.StartDate);
            var orderedProgramsToIngest = programsToIngest.OrderBy(p => p.StartDate);
            var firstCurrentProgram = currentPrograms.FirstOrDefault();
            var firstProgramToIngest = orderedProgramsToIngest.FirstOrDefault(p => p.EpgId != firstCurrentProgram?.EpgId);
            var lastCurrentProgram = orderedCurrentPrograms.LastOrDefault();
            var lastProgramToIngest = orderedProgramsToIngest.LastOrDefault(p => p.EpgId != lastCurrentProgram?.EpgId);

            _Logger.Debug($"CutSourceOrTargetOverlappingDates > firstCurrentProgram:[{firstCurrentProgram}],lastCurrentProgram:[{lastCurrentProgram}],firstProgramToIngest:[{firstProgramToIngest}],lastProgramToIngest:[{lastProgramToIngest}]");
            if (firstCurrentProgram != null && firstProgramToIngest != null)
            {
                if (firstCurrentProgram.EndDate > firstProgramToIngest.StartDate)
                {
                    if (_IngestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.CutTarget)
                    {
                        var msg = $"Program [{firstProgramToIngest.EpgExternalId}] overlapping [{firstCurrentProgram.EpgExternalId}, cutting current programs end date from [{firstCurrentProgram.EndDate}], to [{firstProgramToIngest.StartDate}]";
                        _Results[firstProgramToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        _Logger.Debug(msg);
                        var firstCurrentProgramTranslations = GetAllProgramsTranslations(firstCurrentProgram.EpgId);
                        firstCurrentProgramTranslations.ForEach(p => p.EndDate = firstProgramToIngest.StartDate);
                        firstCurrentProgram.EpgCbObjects = firstCurrentProgramTranslations;

                        result.Add(firstCurrentProgram);
                    }
                    else
                    {
                        var msg = $"Program [{firstProgramToIngest.EpgExternalId}] overlapping [{firstCurrentProgram.EpgExternalId}, cutting program to ingest start date from [{firstProgramToIngest.StartDate}] to [{firstCurrentProgram.EndDate}]";
                        _Results[firstProgramToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        _Logger.Debug(msg);
                        firstProgramToIngest.StartDate = firstCurrentProgram.EndDate;
                    }
                }
            }

            if (lastCurrentProgram != null && lastProgramToIngest != null)
            {
                if (lastCurrentProgram.StartDate < lastProgramToIngest.EndDate)
                {
                    if (_IngestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.CutTarget)
                    {
                        var msg = $"Program [{lastCurrentProgram.EpgExternalId}] overlapping [{lastProgramToIngest.EpgExternalId}, cutting current programs start date from [{lastCurrentProgram.StartDate}], to [{lastProgramToIngest.EndDate}]";
                        _Results[lastProgramToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        _Logger.Debug(msg);
                        var lastCurrentProgramTranslations = GetAllProgramsTranslations(lastCurrentProgram.EpgId);
                        lastCurrentProgramTranslations.ForEach(p => p.StartDate = lastProgramToIngest.EndDate);
                        lastCurrentProgram.EpgCbObjects = lastCurrentProgramTranslations;
                        result.Add(lastCurrentProgram);
                    }
                    else
                    {
                        var msg = $"Program [{lastCurrentProgram.EpgExternalId}] overlapping [{lastProgramToIngest.EpgExternalId}, cutting program to ingest end date from [{lastProgramToIngest.EndDate}], to [{lastCurrentProgram.StartDate}]";
                        _Results[lastProgramToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        _Logger.Debug(msg);
                        lastProgramToIngest.EndDate = lastCurrentProgram.StartDate;
                    }
                }
            }

            return result;
        }

        private List<EpgCB> GetAllProgramsTranslations(ulong epgID)
        {
            var documentIds = _EpgBL.GetEpgCBKeys(_EventData.GroupId, (long)epgID, _Languages.Values);
            var result = _CouchbaseManager.GetValues<EpgCB>(documentIds, false);
            return result.Values.ToList();
        }

        private List<EpgProgramBulkUploadObject> CalculateSimulatedFinalStateAfterIngest(IList<EpgProgramBulkUploadObject> programsToAdd, IList<EpgProgramBulkUploadObject> programsToUpdate)
        {
            _Logger.Debug($"CalculateSimulatedFinalStateAfterIngest > adding EpgIds to new programs");
            var result = new List<EpgProgramBulkUploadObject>();
            if (programsToAdd.Any())
            {
                var newIds = _EpgBL.GetNewEpgIds(programsToAdd.Count).ToList();
                for (int i = 0; i < programsToAdd.Count; i++)
                {
                    var idToSet = newIds[i];
                    programsToAdd[i].EpgId = idToSet;
                    programsToAdd[i].EpgCbObjects.ForEach(p => p.EpgID = idToSet);
                }

                result.AddRange(programsToAdd);
            }

            result.AddRange(programsToUpdate);

            result = result.OrderBy(program => program.StartDate).ToList();

            return result;
        }

        private bool HandleOverlapsAndGaps(CRUDOperations<EpgProgramBulkUploadObject> crudOperations, Dictionary<string, BulkUploadProgramAssetResult> programAssetResultsDictionary)
        {
            bool isValid = true;

            List<EpgProgramBulkUploadObject> calculatedPrograms = crudOperations.ItemsToAdd.Concat(crudOperations.ItemsToUpdate).ToList();
            IDictionary<string, EpgCB> autoFillEpgsCB = null;

            if (_Languages?.Keys.Count > 0)
            {
                List<string> autoFillKeys = new List<string>();
                autoFillKeys = _Languages.Keys.Select(s => GetAutoFillKey(_BulkUploadObject.GroupId, s)).ToList();
                // Get AutoFill default program
                autoFillEpgsCB = _CouchbaseManager.GetValues<EpgCB>(autoFillKeys, true);
            }

            for (int programIndex = 0; programIndex < calculatedPrograms.Count - 1; programIndex++)
            {
                var currentProgram = calculatedPrograms[programIndex];

                // we check the next SEVERAL programs because some of them might be overlapping the current one
                for (int secondaryIndex = programIndex + 1; (secondaryIndex < calculatedPrograms.Count) && isValid; secondaryIndex++)
                {
                    var nextProgram = calculatedPrograms[secondaryIndex];

                    // if the current program ends when the next one starts - we're valid
                    if (currentProgram.EndDate != nextProgram.StartDate)
                    {
                        // if the next program starts before the current ends, it means we have an overlap
                        if (currentProgram.EndDate > nextProgram.StartDate)
                        {
                            if (_IngestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.Reject)
                            {
                                programAssetResultsDictionary[currentProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program overlap");
                                programAssetResultsDictionary[nextProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program overlap");

                                isValid = false;
                            }
                        }
                        // if the next program starts after the current ends, it means we have a gap
                        else
                        {
                            switch (_IngestProfile.DefaultAutoFillPolicy)
                            {
                                case eIngestProfileAutofillPolicy.Reject:
                                    programAssetResultsDictionary[currentProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program gap");
                                    programAssetResultsDictionary[nextProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program gap");

                                    isValid = false;
                                    break;
                                case eIngestProfileAutofillPolicy.Autofill:

                                    if (_IngestProfile.OverlapChannels == null || _IngestProfile.OverlapChannels.Contains(currentProgram.ChannelId))
                                    {
                                        if (autoFillEpgsCB?.Count > 0)
                                        {
                                            foreach (var item in autoFillEpgsCB)
                                            {
                                                var defaultGapProgram = GetDefaultAutoFillProgram(item.Value, item.Value.Language, currentProgram.EndDate, nextProgram.StartDate, currentProgram.ChannelId);
                                                if (defaultGapProgram != null)
                                                {
                                                    crudOperations.ItemsToAdd.Add(defaultGapProgram);
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case eIngestProfileAutofillPolicy.KeepHoles:
                                    break;
                                default:
                                    break;
                            }
                        }

                        break;
                    }
                }
            }

            return isValid;
        }

        private EpgProgramBulkUploadObject GetDefaultAutoFillProgram(EpgCB autoFillProgram, string langCode, DateTime start, DateTime end, int channelId)
        {
            if (autoFillProgram != null)
            {
                EpgCB autoFillEpgCB = ObjectCopier.Clone(autoFillProgram);
                var epgExternalId = Guid.NewGuid().ToString();

                autoFillEpgCB.StartDate = start;
                autoFillEpgCB.EndDate = end;
                autoFillEpgCB.ChannelID = channelId;
                autoFillEpgCB.EpgIdentifier = epgExternalId;
                autoFillEpgCB.Crid = epgExternalId;
                autoFillEpgCB.DocumentId = GetEpgCBDocumentId(epgExternalId, langCode, _BulkUploadObject.Id);
                autoFillEpgCB.EnableCDVR = 0;
                autoFillEpgCB.EnableCatchUp = 0;
                autoFillEpgCB.EnableStartOver = 0;
                autoFillEpgCB.EnableTrickPlay = 0;

                return new EpgProgramBulkUploadObject()
                {
                    StartDate = start,
                    EndDate = end,
                    IsAutoFill = true,
                    ChannelId = channelId,
                    EpgExternalId = epgExternalId,
                    ParentGroupId = _BulkUploadObject.GroupId,
                    GroupId = _BulkUploadObject.GroupId,
                    EpgCbObjects = new List<EpgCB>() { autoFillEpgCB }
                };
            }
            else
            {
                return null;
            }
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
            string source = BulkUploadMethods.GetIngestCurrentProgramsAliasName(_EventData.GroupId, _EventData.DateOfProgramsToIngest);
            string destination = BulkUploadMethods.GetIngestDraftTargetIndexName(_EventData.GroupId, _EventData.BulkUploadId, _EventData.DateOfProgramsToIngest);

            if (_ElasticSearchClient.IndexExists(source))
            {
                CloneAndReindexData(source, destination);
            }
            else
            {
                BuildNewIndex(destination);
            }


            return result;
        }

        private void CloneAndReindexData(string source, string destination)
        {
            var isCloneSuccess = _ElasticSearchClient.CloneIndexWithoutData(source, destination);
            if (isCloneSuccess)
            {
                var isReindexSuccess = _ElasticSearchClient.Reindex(source, destination);
                if (!isReindexSuccess) { _Logger.ErrorFormat($"Reindex {source} to {destination} failure"); }
            }
            else
            {
                _Logger.ErrorFormat($"Reindex {source} to {destination} failure");
            }

            _Logger.Debug($"Clone and Reindex {source} to {destination} success");
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
            GetAnalyzers(out var analyzers, out var filters, out var tokenizers);

            var sizeOfBulk = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;
            if (sizeOfBulk == 0) { sizeOfBulk = 50; }

            var maxResults = ApplicationConfiguration.ElasticSearchConfiguration.MaxResults.IntValue;
            if (maxResults == 0) { maxResults = 100000; }

            var success = _ElasticSearchClient.BuildIndex(newIndexName, 0, 0, analyzers, filters, tokenizers, maxResults);
            return success;
        }

        private void GetAnalyzers(out List<string> analyzers, out List<string> filters, out List<string> tokenizers)
        {
            analyzers = new List<string>();
            filters = new List<string>();
            tokenizers = new List<string>();

            if (_Languages?.Values != null)
            {
                foreach (var language in _Languages.Values)
                {
                    string analyzer = ElasticSearchApi.GetAnalyzerDefinition(ElasticSearch.Common.Utils.GetLangCodeAnalyzerKey(language.Code, ANALYZER_VERSION));
                    string filter = ElasticSearchApi.GetFilterDefinition(ElasticSearch.Common.Utils.GetLangCodeFilterKey(language.Code, ANALYZER_VERSION));
                    string tokenizer = ElasticSearchApi.GetTokenizerDefinition(ElasticSearch.Common.Utils.GetLangCodeTokenizerKey(language.Code, ANALYZER_VERSION));

                    if (string.IsNullOrEmpty(analyzer))
                    {
                        _Logger.Error(string.Format("analyzer for language {0} doesn't exist", language.Code));
                    }
                    else
                    {
                        analyzers.Add(analyzer);
                    }

                    if (!string.IsNullOrEmpty(filter))
                    {
                        filters.Add(filter);
                    }

                    if (!string.IsNullOrEmpty(tokenizer))
                    {
                        tokenizers.Add(tokenizer);
                    }
                }

                // we always want a lowercase analyzer
                analyzers.Add(LOWERCASE_ANALYZER);

                // we always want "autocomplete" ability
                filters.Add(PHRASE_STARTS_WITH_FILTER);
                analyzers.Add(PHRASE_STARTS_WITH_ANALYZER);
                analyzers.Add(PHRASE_STARTS_WITH_SEARCH_ANALYZER);

            }
        }

        private string GetAutoFillKey(object groupId, string langCode)
        {
            return $"autofill_{groupId}_{langCode}";
        }
    }
}