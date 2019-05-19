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
using ApiObjects.Response;
using ConfigurationManager;
using Polly;
using Polly.Retry;
using ApiObjects.SearchObjects;
using ApiLogic;
using Core.GroupManagers;
using TVinciShared;
using ApiObjects.Epg;
using EpgBL;
using ESUtils = ElasticSearch.Common.Utils;
using System.Globalization;
using ApiObjects.Catalog;
using Tvinci.Core.DAL;

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string DEFAULT_INDEX_MAPPING_TYPE = "epg";
        public static readonly int DEFAULT_CATCHUP_DAYS = 7;
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
        private static readonly int EXPIRY_DATE_DELTA = (ApplicationConfiguration.EPGDocumentExpiry.IntValue > 0) ? ApplicationConfiguration.EPGDocumentExpiry.IntValue : 7;

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
                _BulkUploadObject = GetBulkUploadData();
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

                var minStartDate = programsToIngest.Min(p => p.StartDate);
                var maxEndDate = programsToIngest.Max(p => p.EndDate);
                var currentPrograms = GetCurrentProgramsByDate(minStartDate, maxEndDate);

                var crudOperations = CalculateCRUDOperations(currentPrograms, programsToIngest);

                var edgeProgramsToUpdate = CalculateRequiredUpdatesToEdgesDueToOverlap(currentPrograms, programsToIngest, crudOperations);

                var finalEpgState = CalculateSimulatedFinalStateAfterIngest(crudOperations.ItemsToAdd, crudOperations.ItemsToUpdate);

                ValidateProgramDates(finalEpgState, _Results);
                await UpdateCouchbase(finalEpgState, _Results);

                CloneExistingIndex();
                UpdateClonedIndex(finalEpgState, crudOperations.ItemsToDelete);

                var indexIsValid = ValidateClonedIndex(finalEpgState);

                if (indexIsValid)
                {
                    UpdateBulkUploadResults(_Results, finalEpgState);
                    SwitchAliases();
                    BulkUploadManager.UpdateBulkUploadResults(_Results.Values);
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Success);

                    // Update edgs if there are any updates to be made dure to overlap
                    if (edgeProgramsToUpdate.Any())
                    {
                        await UpdateCouchbase(edgeProgramsToUpdate, _Results);
                        UpdateClonedIndex(edgeProgramsToUpdate, new List<EpgProgramBulkUploadObject>());
                    }

                }
                else
                {
                    BulkUploadManager.UpdateBulkUploadResults(_Results.Values);
                    BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Failed);
                }

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

        private void UpdateBulkUploadResults(Dictionary<string, BulkUploadProgramAssetResult> results, List<EpgProgramBulkUploadObject> finalEpgState)
        {
            foreach (var prog in finalEpgState)
            {
                var resultObj = results[prog.EpgExternalId];
                resultObj.ObjectId = (long)prog.EpgId;
                resultObj.Status = BulkUploadResultStatus.Ok;
                // TODO: allow updating results in bulk
                //BulkUploadManager.UpdateBulkUploadResult(_EventData.GroupId, _BulkUploadObject.Id, resultObj.Index, Status.Ok, resultObj.ObjectId, resultObj.Warnings);
            }
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

            UploadEpgItemImages(parsedProg.icon, epgItem, bulkUploadResultItem);

            return epgItem;
        }

        private static void SetSearchEndDate(List<EpgCB> lEpg, int groupID)
        {
            try
            {
                var days = ApplicationConfiguration.CatalogLogicConfiguration.CurrentRequestDaysOffset.IntValue;
                days = days == 0 ? DEFAULT_CATCHUP_DAYS : days;

                List<string> epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList<string>();
                var linearChannelSettings = BulkUploadIngestJobData.GetLinearChannelSettings(groupID, epgChannelIds);

                Parallel.ForEach(lEpg.Cast<EpgCB>(), currentElement =>
                {
                    var channel = linearChannelSettings.FirstOrDefault(c => c.ChannelID.Equals(currentElement.ChannelID));
                    if (channel == null)
                    {
                        currentElement.SearchEndDate = currentElement.EndDate.AddDays(days);
                    }
                    else if (channel.EnableCatchUp)
                    {
                        currentElement.SearchEndDate =
                            currentElement.EndDate.AddMinutes(channel.CatchUpBuffer);
                    }
                    else
                    {
                        currentElement.SearchEndDate = currentElement.EndDate;
                    }
                });
            }
            catch (Exception ex)
            {
                _Logger.Error($"Error EPG ingest threw an exception. (in GetSearchEndDate). Exception={ex.Message};Stack={ex.StackTrace}", ex);
                throw ex;
            }
        }

        private void UploadEpgItemImages(icon[] icons, EpgCB epgItem, BulkUploadProgramAssetResult bulkUploadResultItem)
        {
            var result = new List<EpgPicture>();
            foreach (var icon in icons)
            {
                var epgPicture = new EpgPicture();
                var imgUrl = icon.src;
                long ratio = 0;
                long imageTypeId = 0;


                if (_IsOpc)
                {
                    if (_GroupRatioNamesToImageTypes.TryGetValue(icon.ratio, out var imgType))
                    {
                        imageTypeId = imgType.Id;
                        ratio = imgType.RatioId.Value;
                    }

                }
                else
                {
                    ratio = long.Parse(_NonOpcGroupRatios.FirstOrDefault(r => r.Value == icon.ratio).Key);
                }

                if (!string.IsNullOrEmpty(imgUrl))
                {
                    int picId = EpgImageManager.DownloadEPGPic(imgUrl, epgItem.Name, epgItem.GroupID, 0, epgItem.ChannelID, ratio, imageTypeId);
                    if (picId != 0)
                    {
                        var baseURl = ODBCWrapper.Utils.GetTableSingleVal("epg_pics", "BASE_URL", picId);
                        if (baseURl != null && baseURl != DBNull.Value)
                        {
                            epgPicture.Url = baseURl.ToString();
                            epgPicture.PicID = picId;
                            epgPicture.Ratio = icon.ratio;
                            epgPicture.ImageTypeId = imageTypeId;
                        }
                    }
                    else
                    {
                        bulkUploadResultItem.AddWarning((int)IngestWarnings.FailedDownloadPic, "Failed to download Epg picture");
                    }
                }
            }




        }

        private static string GetEpgCBDocumentId(string external_id, string langCode, long bulkUploadId)
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

            var targetIndex = GetIngestDraftTargetIndexName();
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

        private BulkUpload GetBulkUploadData()
        {
            var bulkUploadData = BulkUploadManager.GetBulkUpload(_EventData.GroupId, _EventData.BulkUploadId);

            if (bulkUploadData?.Object == null)
            {
                string message = string.Empty;

                if (bulkUploadData != null && bulkUploadData.Status != null)
                {
                    message = bulkUploadData.Status.Message;
                }

                throw new Exception($"Received invalid bulk upload. group id = {_EventData.GroupId} id = {_EventData.BulkUploadId} message = {message}");
            }

            return bulkUploadData.Object;
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
            // type represens the language, the default is the main language
            string type = DEFAULT_INDEX_MAPPING_TYPE;

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

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

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

                    result.Add(epgItem);
                }
            }

            return result;
        }

        private CRUDOperations<EpgProgramBulkUploadObject> CalculateCRUDOperations(IList<EpgProgramBulkUploadObject> currentPrograms, IList<EpgProgramBulkUploadObject> programsToIngest)
        {
            _Logger.Debug($"CalculateCRUDOperations > currentPrograms.count:[{currentPrograms.Count}] programsToIngest.count:[{programsToIngest.Count}]");
            var crudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();

            var currentProgramsDictionary = currentPrograms.ToDictionary(epg => epg.EpgExternalId);
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
            crudOperations.ItemsToDelete = currentProgramsDictionary.Values.ToList();

            _Logger.Debug($"CalculateCRUDOperations > add:[{crudOperations.ItemsToAdd.Count}], update:[{crudOperations.ItemsToUpdate.Count}], delete:[{crudOperations.ItemsToDelete.Count}]");

            return crudOperations;
        }

        private List<EpgProgramBulkUploadObject> CalculateRequiredUpdatesToEdgesDueToOverlap(IList<EpgProgramBulkUploadObject> currentPrograms, IList<EpgProgramBulkUploadObject> programsToIngest, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var result = new List<EpgProgramBulkUploadObject>();
            // If overlaps are allowed we have to check the edges of the give range for overlap and cut the source or the target
            if (_IngestProfile.DefaultOverlapPolicy != eIngestProfileOverlapPolicy.Reject)
            {
                _Logger.Debug($"CalculateCRUDOperations > _IngestProfile.DefaultOverlapPolicy:[{_IngestProfile.DefaultOverlapPolicy}], calculating required update to edge programs");
                result = CutSourceOrTargetOverlappingDates(currentPrograms, programsToIngest);
                _Logger.Debug($"CalculateCRUDOperations > after edge overlap calculations add:[{crudOperations.ItemsToAdd.Count}], update:[{crudOperations.ItemsToUpdate.Count}], delete:[{crudOperations.ItemsToDelete.Count}]");
            }

            return result;
        }

        private List<EpgProgramBulkUploadObject> CutSourceOrTargetOverlappingDates(IList<EpgProgramBulkUploadObject> currentPrograms, IList<EpgProgramBulkUploadObject> programsToIngest)
        {
            var result = new List<EpgProgramBulkUploadObject>();
            var orderedCurrentPrograms = currentPrograms.OrderBy(p => p.StartDate);
            var orderedProgramsToIngest = programsToIngest.OrderBy(p => p.StartDate);
            var firstCurrentProgram = currentPrograms.FirstOrDefault();
            var firstProgramToIngest = orderedProgramsToIngest.FirstOrDefault();
            var lastCurrentProgram = orderedCurrentPrograms.LastOrDefault();
            var lastProgramToIngest = orderedProgramsToIngest.LastOrDefault();

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
                        _Results[firstProgramToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        _Logger.Debug(msg);
                        var lastCurrentProgramTranslations = GetAllProgramsTranslations(lastCurrentProgram.EpgId);
                        lastCurrentProgramTranslations.ForEach(p => p.StartDate = lastProgramToIngest.EndDate);
                        lastCurrentProgram.EpgCbObjects = lastCurrentProgramTranslations;
                        result.Add(lastCurrentProgram);
                    }
                    else
                    {
                        var msg = $"Program [{lastCurrentProgram.EpgExternalId}] overlapping [{lastProgramToIngest.EpgExternalId}, cutting program to ingest end date from [{lastProgramToIngest.EndDate}], to [{lastCurrentProgram.StartDate}]";
                        _Results[firstProgramToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
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

        private bool ValidateProgramDates(List<EpgProgramBulkUploadObject> calculatedPrograms, Dictionary<string, BulkUploadProgramAssetResult> programAssetResultsDictionary)
        {
            // TODO: consider returning false if should reject .. review ...
            bool isValid = true;

            bool checkOverlap = _IngestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.Reject;
            bool checkGaps = _IngestProfile.DefaultAutoFillPolicy == eIngestProfileAutofillPolicy.Reject;

            // if at least one of the policies means rejecting, we will go over the programs and validate them
            if (checkOverlap || checkGaps)
            {
                for (int programIndex = 0; programIndex < calculatedPrograms.Count - 1; programIndex++)
                {
                    var currentProgram = calculatedPrograms[programIndex];

                    bool continueCheckingOverlap = true;

                    // we check the next SEVERAL programs because some of them might be overlapping the current one
                    for (int secondaryIndex = programIndex + 1; (secondaryIndex < calculatedPrograms.Count) && continueCheckingOverlap; secondaryIndex++)
                    {
                        var nextProgram = calculatedPrograms[secondaryIndex];

                        // if the current program ends when the next one starts - we're valid
                        // TODO minor : only equals? we need to think about it
                        if (currentProgram.EndDate == nextProgram.StartDate)
                        {
                            // we don't need to continue checking overlaps/gaps if the programs are valid
                            continueCheckingOverlap = false;
                        }
                        // if they're different, we're not valid. let's see if it's a gap or an overlap
                        else
                        {
                            // if the next program starts before the current ends, it means we have an overlap
                            if (checkOverlap && currentProgram.EndDate > nextProgram.StartDate)
                            {
                                programAssetResultsDictionary[currentProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program overlap");
                                programAssetResultsDictionary[nextProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program overlap");
                            }

                            // if the next program starts after the current ends, it means we have a gap
                            if (currentProgram.StartDate > currentProgram.EndDate)
                            {
                                // we don't need to check anymore overlaps when we have a gap
                                continueCheckingOverlap = false;

                                if (checkGaps)
                                {
                                    programAssetResultsDictionary[currentProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program gap");
                                    programAssetResultsDictionary[nextProgram.EpgExternalId].AddError(eResponseStatus.EPGSProgramDatesError, "Program gap");
                                }
                            }
                        }
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// create new documents for ALL epgs - generate document key {epg_id}_{language}_{bulk_upload_id}
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="calculatedPrograms"></param>
        /// <param name="bulkUploadId"></param>
        private async Task UpdateCouchbase(List<EpgProgramBulkUploadObject> calculatedPrograms, Dictionary<string, BulkUploadProgramAssetResult> programAssetResultsDictionary)
        {
            var dal = new EpgDal_Couchbase(_EventData.GroupId);
            // tcm configurable?
            int retryCount = 3;
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (ex, time, attempt, ctx) =>
                {
                    // TODO: improve logging
                    _Logger.Warn("Error while trying to upsert EPG to couchbase", ex);
                    _Logger.Warn($"couchbase upsert retry attempt:[{attempt}/{retryCount}]");
                }
            );

            var insertResult = false;
            await policy.ExecuteAsync(async () =>
            {
                var epgCbObjectToInsert = calculatedPrograms.SelectMany(p => p.EpgCbObjects).ToList();
                SetSearchEndDate(epgCbObjectToInsert, _EventData.GroupId);
                insertResult = await dal.InsertPrograms(epgCbObjectToInsert, EXPIRY_DATE_DELTA);

            });

            if (!insertResult)
            {
                _Logger.Error($"Failed inserting program. group:[{_EventData.GroupId}] bulkUploadId:[{_EventData.GroupId}]");
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
            string source = GetIngestCurrentProgramsAliasName();
            string destination = GetIngestDraftTargetIndexName();

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

        private void UpdateClonedIndex(IList<EpgProgramBulkUploadObject> calculatedPrograms, IList<EpgProgramBulkUploadObject> programsToDelete)
        {
            var bulkSize = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;
            var index = GetIngestDraftTargetIndexName();
            var bulkRequests = new List<ESBulkRequestObj<string>>();
            var serializer = new ESSerializerV2();
            var isOpc = GroupSettingsManager.IsOpc(_EventData.GroupId);
            var metasToPad = GetMetasToPad(_EventData.GroupId);

            var programTranslationsToIndex = calculatedPrograms.SelectMany(p => p.EpgCbObjects);
            foreach (var program in programTranslationsToIndex)
            {
                program.PadMetas(metasToPad);
                var suffix = program.Language;
                var language = _Languages[program.Language];

                // Serialize EPG object to string
                var serializedEpg = serializer.SerializeEpgObject(program, suffix, isOpc);
                var epgType = GetTanslationType(DEFAULT_INDEX_MAPPING_TYPE, language);

                var totalMinutes = GetTTLMinutes(program);
                // TODO: what should we do if someone trys to ingest something to the past ... :\
                totalMinutes = totalMinutes < 0 ? 10 : totalMinutes;

                var ttl = string.Format("{0}m", totalMinutes);

                var bulkRequest = new ESBulkRequestObj<string>()
                {
                    docID = program.EpgID.ToString(),
                    document = serializedEpg,
                    index = index,
                    Operation = eOperation.index,
                    routing = program.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                    type = epgType,
                    ttl = ttl
                };

                bulkRequests.Add(bulkRequest);

                // If we exceeded maximum size of bulk 
                if (bulkRequests.Count >= bulkSize)
                {
                    // create bulk request now and clear list
                    var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                    if (invalidResults != null && invalidResults.Count > 0)
                    {
                        foreach (var item in invalidResults)
                        {
                            _Logger.Error($"Could not add EPG to ES index. GroupID={_EventData.GroupId} epgId={item.Key} error={item.Value}");
                        }
                    }

                    bulkRequests.Clear();
                }
            }

            var programIds = programsToDelete.Select(program => program.EpgId);
            _Logger.Debug($"Update elasticsearch index completed, delteting required docuements. documents.leng:[{programsToDelete.Count}]");
            if (programIds.Any())
            {
                var deleteQuery = GetElasticsearchQueryForEpgIDs(programIds);
                _ElasticSearchClient.DeleteDocsByQuery(index, "", ref deleteQuery);
            }

            // If we have anything left that is less than the size of the bulk
            if (bulkRequests.Count > 0)
            {
                var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var item in invalidResults)
                    {
                        _Logger.Error($"Could not add EPG to ES index. GroupID={_EventData.GroupId} epgId={item.Key} error={item.Value}");
                    }
                }
            }
        }

        /// <summary>
        /// TODO: DO THIS
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        private HashSet<string> GetMetasToPad(int groupId)
        {
            return new HashSet<string>();
        }

        private bool ValidateClonedIndex(List<EpgProgramBulkUploadObject> calculatedPrograms)
        {
            // Wait time is 2 sec + 50ms for every program that was indexed
            // TODO: make configurable
            var delayMsBeforeValidation = 2000 + (calculatedPrograms.Count * 10);
            var result = false;
            int retryCount = 5; // TODO: Tcm configuration?

            var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
                {
                    // TODO: improve logging
                    _Logger.Warn($"Validation Attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
                }
            );

            var index = GetIngestDraftTargetIndexName();

            // Checking all languages by searhcing for all types
            var type = string.Empty;
            var isValid = true;
            policy.Execute(() =>
            {
                isValid = true;
                var programIds = calculatedPrograms.Select(program => program.EpgId);
                var searchQuery = GetElasticsearchQueryForEpgIDs(programIds);

                var searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

                var jsonResult = JObject.Parse(searchResult);
                var tempToken = jsonResult.SelectToken("hits.total");
                int totalItems = tempToken?.Value<int>() ?? 0;

                var expectedItemsCount = calculatedPrograms.SelectMany(p => p.EpgCbObjects).Count();
                if (totalItems != expectedItemsCount)
                {
                    isValid = false;
                }

                // TODO : explain failures


                if (!isValid)
                {
                    _Logger.Warn($"Missing program from ES index.");
                    throw new Exception("Missing program from ES index");
                }
            });

            result = isValid;

            return result;
        }

        private static string GetElasticsearchQueryForEpgIDs(IEnumerable<ulong> programIds)
        {
            // Build query for getting programs
            var query = new FilteredQuery(true);
            var filter = new QueryFilter();

            // basic initialization
            query.PageIndex = 0;
            query.PageSize = 1;
            query.ReturnFields.Clear();

            var composite = new FilterCompositeType(CutWith.AND);

            // build terms query: epg_id IN (1, 2, 3 ... bulkSize)

            var terms = ESTerms.GetSimpleNumericTerm("epg_id", programIds);
            composite.AddChild(terms);

            filter.FilterSettings = composite;
            query.Filter = filter;

            var searchQuery = query.ToString();
            return searchQuery;
        }

        /// <summary>
        /// switch aliases - 
        /// delete epg_203_20190422 for epg_203_20190422_old_bulk_upload_id
        /// add epg_203_20190422 for epg_203_20190422_current_bulk_upload_id
        /// </summary>
        /// <param name="bulkUploadId"></param>
        /// <param name="dateOfIngest"></param>
        private void SwitchAliases()
        {
            string currentProgramsAlias = GetIngestCurrentProgramsAliasName();
            string globalAlias = _EpgBL.GetProgramIndexAlias();


            // Should only be one but we will loop anyway ...
            var previousIndices = _ElasticSearchClient.GetAliases(currentProgramsAlias);
            _Logger.Debug($"Removing alias:[{currentProgramsAlias}, {globalAlias}] from:[{string.Join(",", previousIndices)}].");
            foreach (var index in previousIndices)
            {
                _ElasticSearchClient.RemoveAlias(index, globalAlias);
                _ElasticSearchClient.RemoveAlias(index, currentProgramsAlias);
            }



            string newIndex = GetIngestDraftTargetIndexName();
            _Logger.Debug($"Adding alias:[{currentProgramsAlias}, {globalAlias}] To:[{string.Join(",", newIndex)}].");
            _ElasticSearchClient.AddAlias(newIndex, currentProgramsAlias);
            _ElasticSearchClient.AddAlias(newIndex, globalAlias);
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

        /// <summary>
        /// This is the main alias of all programs
        /// </summary>
        //private string GetProgramIndexAlias()
        //{

        //    return $"{_EventData.GroupId}_epg_v2";
        //}

        /// <summary>
        /// This is the index name of exsisting programs
        /// </summary>
        private string GetIngestCurrentProgramsAliasName()
        {
            string dateString = _EventData.DateOfProgramsToIngest.ToString(ESUtils.ES_DATEONLY_FORMAT);
            return $"{_EventData.GroupId}_epg_v2_{dateString}";
        }

        /// <summary>
        /// This is the index name that we will ingest into
        /// </summary>
        private string GetIngestDraftTargetIndexName()
        {
            string dateString = _EventData.DateOfProgramsToIngest.ToString(ESUtils.ES_DATEONLY_FORMAT);
            return $"{_EventData.GroupId}_epg_v2_{dateString}_{_EventData.BulkUploadId}";
        }

        public static string GetTanslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }
            else
            {
                return string.Concat(type, "_", language.Code);
            }
        }

        private bool BuildNewIndex(string newIndexName)
        {
            GetIngestDraftTargetIndexName();
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

        protected virtual double GetTTLMinutes(EpgCB epg)
        {
            return Math.Ceiling((epg.EndDate.AddDays(EXPIRY_DATE_DELTA) - DateTime.UtcNow).TotalMinutes);
        }
    }

}
