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

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string DEFAULT_INDEX_MAPPING_TYPE = "epg";
        private static readonly string EPG_SEQUENCE_DOCUMENT = "epg_sequence_document";
        private static readonly int EXPIRY_DATE_DELTA = (ApplicationConfiguration.EPGDocumentExpiry.IntValue > 0) ? ApplicationConfiguration.EPGDocumentExpiry.IntValue : 7;

        private readonly ElasticSearchApi _ElasticSearchClient = null;
        private readonly CouchbaseManager.CouchbaseManager _CouchbaseManager = null;

        private TvinciEpgBL _EpgBL;
        private BulkUploadIngestEvent _EventData;
        private BulkUpload _BulkUploadObject;
        private BulkUploadIngestJobData _BulkUploadJobData;
        private IngestProfile _IngestProfile;
        private IDictionary<string, LanguageObj> _Languages;
        private LanguageObj _DefaultLanguage;

        public BulkUploadIngestHandler()
        {
            _ElasticSearchClient = new ElasticSearchApi();
            _CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);
        }

        public async Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                log.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                _EpgBL = new TvinciEpgBL(serviceEvent.GroupId);
                _EventData = serviceEvent;
                _BulkUploadObject = GetBulkUploadData();
                _BulkUploadJobData = _BulkUploadObject.JobData as BulkUploadIngestJobData;
                _IngestProfile = GetIngestProfile(_BulkUploadJobData.IngestProfileId);
                _Languages = GetGroupLanguages(out _DefaultLanguage);

                ValidateServiceEvent();

                var results = GetProgramAssetResults();
                AddEpgCBObjects(results);
                var epgBulkUploadObjects = results.Values.Select(r => r.Object).Cast<EpgProgramBulkUploadObject>();

                var minStartDate = epgBulkUploadObjects.Min(p => p.StartDate);
                var maxEndDate = epgBulkUploadObjects.Max(p => p.EndDate);
                var currentPrograms = GetCurrentProgramsByDate(minStartDate, maxEndDate);
                var programsToIngest = epgBulkUploadObjects.SelectMany(p => p.EpgCbObjects).ToList();

                var crudOperations = CalculateCRUDOperations(currentPrograms, programsToIngest);
                var finalEpgState = CalculateSimulatedFinalStateAfterIngest(crudOperations.ItemsToAdd, crudOperations.ItemsToUpdate);

                // TODO: validate policies
                // TODO: update edges if policy requires to cut source or target;
                await UpdateCouchbase(finalEpgState, results);

                CloneExistingIndex();
                UpdateClonedIndex(finalEpgState, crudOperations.ItemsToDelete);
                
                // TODO: validate index
                // TODO: switch alias

            }
            catch (Exception ex)
            {
                log.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                throw;
            }

            return;
        }

        private void AddEpgCBObjects(Dictionary<string, BulkUploadProgramAssetResult> results)
        {
            // TODO: move language getting to the right place

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

        private static EpgCB GetEpgCBObject(string langCode, string defaultLangCode, EpgProgramBulkUploadObject prog, BulkUploadProgramAssetResult bulkUploadResultItem)
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
            epgItem.pictures = parsedProg.icon.Select(p => new EpgPicture
            {
                // TODO Upload picture
                Url = p.src,
                Ratio = p.ratio,
                PicWidth = p.width,
                PicHeight = p.height,
                ImageTypeId = 0, // TODO: Atthur\sunny look at the code in ws_ingest ingets.cs line#338
            }).ToList();

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
            return epgItem;
        }

        private static string GetEpgCBDocumentId(string external_id, string langCode, long bulkUploadId)
        {
            // TODO: validate if key need specific prefix
            return $"epg_{bulkUploadId}_{langCode}_{external_id}";
        }

        private void ValidateServiceEvent()
        {

            if (_EventData.ProgramsToIngest?.Any() != true)
            {
                throw new Exception($"Received bulk upload ingest event with null or empty programs to insert. group id = {_EventData.GroupId} id = {_EventData.BulkUploadId}");
            }

            if (_EventData.ProgramsToIngest.Count() == 0)
            {
                log.Warn($"Received bulk upload ingest event with 0 programs to insert. group id ={_EventData.GroupId} id = {_EventData.BulkUploadId}");
            }
        }

        private Dictionary<string, BulkUploadProgramAssetResult> GetProgramAssetResults()
        {
            var programsToIngest = _EventData.ProgramsToIngest;
            var bulkUploadResultsDictionary = _BulkUploadObject.Results.Cast<BulkUploadProgramAssetResult>().ToDictionary(k => k.ProgramExternalId);

            var programAssetResultsDictionary = _BulkUploadObject.Results.Cast<BulkUploadProgramAssetResult>().ToDictionary(program => program.ProgramExternalId);

            foreach (var programToIngest in programsToIngest)
            {
                programAssetResultsDictionary[programToIngest.ParsedProgramObject.external_id].Object = programToIngest;
            }

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

        private List<EpgCB> GetCurrentProgramsByDate(DateTime minStartDate, DateTime maxEndDate)
        {
            var result = new List<EpgCB>();
            string index = GetProgramIndexAlias(_EventData.GroupId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_ElasticSearchClient.IndexExists(index)) { return result; }

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
            query.AddReturnField("document_id");
            query.AddReturnField("epg_id");
            query.AddReturnField("start_date");
            query.AddReturnField("end_date");
            query.AddReturnField("epg_identifier");

            // get the epg document ids from elasticsearch
            string searchQuery = query.ToString();
            var searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

            List<string> documentIds = new List<string>();

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                var json = JObject.Parse(searchResult);

                var hits = (json["hits"]["hits"] as JArray);

                foreach (var hit in hits)
                {
                    var epgItem = new EpgCB();
                    epgItem.DocumentId = hit["document_id"]?.ToString();
                    epgItem.EpgIdentifier = hit["epg_identifier"]?.ToString();
                    epgItem.StartDate = hit.Value<DateTime>("start_date");
                    epgItem.EndDate = hit.Value<DateTime>("end_date");
                    epgItem.EpgID = hit.Value<ulong>("epg_id");

                    result.Add(epgItem);
                }
            }

            return result;
        }

        private CRUDOperations<EpgCB> CalculateCRUDOperations(IList<EpgCB> currentPrograms, IList<EpgCB> programsToIngest)
        {
            var crudOperations = new CRUDOperations<EpgCB>
            {
                ItemsToAdd = new List<EpgCB>(),
                ItemsToUpdate = new List<EpgCB>(),
                ItemsToDelete = new List<EpgCB>(),
            };

            var currentProgramsDictionary = currentPrograms.ToDictionary(epg => epg.EpgIdentifier);

            // we cannot use the current programs as a dictioanry becuse there are multiple translation with same external id
            //var programsToIngestDictionary = programsToIngest.ToDictionary(epg => epg.EpgIdentifier);

            foreach (var programToIngest in programsToIngest)
            {
                // if a program exists both on newly ingested epgs and in index - it's an update
                if (currentProgramsDictionary.ContainsKey(programToIngest.EpgIdentifier))
                {
                    // update the epg id of the ingested programs with their existing epg id from CB
                    programToIngest.EpgID = currentProgramsDictionary[programToIngest.EpgIdentifier].EpgID;
                    crudOperations.ItemsToUpdate.Add(programToIngest);
                }
                else
                {
                    // if it exists only on newly ingested epgs and not in index, it's a program to add
                    crudOperations.ItemsToAdd.Add(programToIngest);
                }

                // Remove programs that marked to add or update so that all that is left will be to delete
                currentProgramsDictionary.Remove(programToIngest.EpgIdentifier);
            }

            // all update or add programs were removed form list so we left with items to delete
            crudOperations.ItemsToDelete = currentProgramsDictionary.Values.ToList();

            if (_IngestProfile.DefaultOverlapPolicy != eIngestProfileOverlapPolicy.Reject)
            {
                var orderedCurrentPrograms = currentPrograms.OrderBy(p => p.StartDate);
                var orderedProgramsToIngest = programsToIngest.OrderBy(p => p.StartDate);
                var firstCurrentProgram = currentPrograms.FirstOrDefault();
                var firstProgramToIngest = orderedProgramsToIngest.FirstOrDefault();
                var lastCurrentProgram = orderedCurrentPrograms.LastOrDefault();
                var lastProgramToIngest = orderedProgramsToIngest.LastOrDefault();

                if (firstCurrentProgram != null && firstProgramToIngest != null)
                {
                    if (firstCurrentProgram.EndDate > firstProgramToIngest.StartDate)
                    {
                        // TODO: Get all Current program translation epgCB object
                        if (_IngestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.CutTarget)
                        {
                            firstCurrentProgram.EndDate = firstProgramToIngest.StartDate;
                        }
                        else
                        {
                            firstProgramToIngest.StartDate = firstCurrentProgram.EndDate;
                        }
                    }
                }

                if (lastCurrentProgram != null && lastProgramToIngest != null)
                {
                    if (lastCurrentProgram.StartDate < lastProgramToIngest.EndDate)
                    {
                        // TODO: Get all Current program translation epgCB object
                        if (_IngestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.CutTarget)
                        {
                            lastCurrentProgram.StartDate = lastProgramToIngest.EndDate;
                        }
                        else
                        {
                            lastProgramToIngest.EndDate = lastCurrentProgram.StartDate;
                        }
                    }
                }

            }

            return crudOperations;
        }

        private List<EpgCB> CalculateSimulatedFinalStateAfterIngest(IList<EpgCB> programsToAdd, IList<EpgCB> programsToUpdate)
        {
            var result = new List<EpgCB>();
            _EpgBL.SetEpgIds(programsToAdd);
            result.AddRange(programsToAdd);
            result.AddRange(programsToUpdate);

            // TODO: calculate programs to cut according to policy

            result = result.OrderBy(program => program.StartDate).ToList();

            return result;
        }

        private bool ValidateProgramDates(List<EpgCB> calculatedPrograms, Dictionary<string, BulkUploadProgramAssetResult> programAssetResultsDictionary, IngestProfile ingestProfile)
        {
            // TODO: consider returning false if should reject .. review ...
            bool isValid = true;

            bool checkOverlap = ingestProfile.DefaultOverlapPolicy == eIngestProfileOverlapPolicy.Reject;
            bool checkGaps = ingestProfile.DefaultAutoFillPolicy == eIngestProfileAutofillPolicy.Reject;

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
                                programAssetResultsDictionary[currentProgram.EpgIdentifier].AddError(eResponseStatus.EPGSProgramDatesError, "Program overlap");
                                programAssetResultsDictionary[nextProgram.EpgIdentifier].AddError(eResponseStatus.EPGSProgramDatesError, "Program overlap");
                            }

                            // if the next program starts after the current ends, it means we have a gap
                            if (currentProgram.StartDate > currentProgram.EndDate)
                            {
                                // we don't need to check anymore overlaps when we have a gap
                                continueCheckingOverlap = false;

                                if (checkGaps)
                                {
                                    programAssetResultsDictionary[currentProgram.EpgIdentifier].AddError(eResponseStatus.EPGSProgramDatesError, "Program gap");
                                    programAssetResultsDictionary[nextProgram.EpgIdentifier].AddError(eResponseStatus.EPGSProgramDatesError, "Program gap");
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
        private async Task UpdateCouchbase(List<EpgCB> calculatedPrograms, Dictionary<string, BulkUploadProgramAssetResult> programAssetResultsDictionary)
        {
            var dal = new EpgDal_Couchbase(_EventData.GroupId);
            // tcm configurable?
            int retryCount = 3;
            var policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(retryCount, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)), (ex, time, attempt, ctx) =>
                {
                    // TODO: improve logging
                    log.Warn("Error while trying to upsert EPG to couchbase", ex);
                    log.Warn($"couchbase upsert retry attempt:[{attempt}/{retryCount}]");
                }
            );

            var insertResult = false;
            await policy.ExecuteAsync(async () =>
            {
                insertResult = await dal.InsertPrograms(calculatedPrograms, EXPIRY_DATE_DELTA);

            });

            if (!insertResult)
            {
                log.Error($"Failed inserting program. group:[{_EventData.GroupId}] bulkUploadId:[{_EventData.GroupId}]");
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
            string source = GetProgramIndexDateAlias();
            string destination = GetProgramIndexDateName();

            // need to clone first to have all original settings
            var isCloneSuccess = _ElasticSearchClient.CloneIndexWithoutData(source, destination);
            if (isCloneSuccess)
            {
                var isReindexSuccess = _ElasticSearchClient.Reindex(source, destination);
                if (!isReindexSuccess) { log.ErrorFormat($"Reindex {source} to {destination} failure"); }
            }
            else
            {
                log.ErrorFormat($"Reindex {source} to {destination} failure");
            }

            log.Debug($"Clone and Reindex {source} to {destination} success");

            return result;
        }

        private void UpdateClonedIndex(IList<EpgCB> calculatedPrograms, IList<EpgCB> programsToDelete)
        {
            var bulkSize = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;
            var index = GetProgramIndexDateName();
            var bulkRequests = new List<ESBulkRequestObj<string>>();
            var serializer = new ESSerializerV2();
            var isOpc = GroupSettingsManager.IsOpc(_EventData.GroupId);
            var metasToPad = GetMetasToPad(_EventData.GroupId);

            foreach (var program in calculatedPrograms)
            {
                program.PadMetas(metasToPad);
                var suffix = program.Language;
                var language = _Languages[program.Language];

                // Serialize EPG object to string
                var serializedEpg = serializer.SerializeEpgObject(program, suffix, isOpc);
                var epgType = GetTanslationType(DEFAULT_INDEX_MAPPING_TYPE, language);

                var totalMinutes = GetTTLMinutes(program);
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

                // If we exceeded maximum size of bulk 
                if (bulkRequests.Count >= bulkSize)
                {
                    // create bulk request now and clear list
                    var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                    if (invalidResults != null && invalidResults.Count > 0)
                    {
                        foreach (var item in invalidResults)
                        {
                            log.Error($"Could not add EPG to ES index. GroupID={_EventData.GroupId} epgId={item.Key} error={item.Value}");
                        }
                    }

                    bulkRequests.Clear();
                }
            }

            // TODO: programs to delete

            // If we have anything left that is less than the size of the bulk
            if (bulkRequests.Count > 0)
            {
                var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var item in invalidResults)
                    {
                        log.Error($"Could not add EPG to ES index. GroupID={_EventData.GroupId} epgId={item.Key} error={item.Value}");
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

        private bool ValidateClonedIndex(int groupId, long bulkUploadId, DateTime dateOfIngest, List<EpgCB> calculatedPrograms)
        {
            bool result = false;

            // tcm configurable?
            int retryCount = 3;

            var policy = RetryPolicy.Handle<Exception>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    // TODO: improve logging
                    log.Warn(ex.ToString());
                }
            );

            string index = GetProgramIndexDateName();

            // TODO: LANGUAGEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
            string type = DEFAULT_INDEX_MAPPING_TYPE;
            int bulkSize = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;

            bool isValid = true;

            policy.Execute(() =>
            {
                int skip = 0;

                while (isValid && skip < calculatedPrograms.Count)
                {
                    // Build query for getting programs
                    FilteredQuery query = new FilteredQuery(true);
                    QueryFilter filter = new QueryFilter();

                    // basic initialization
                    query.PageIndex = 0;
                    query.PageSize = 1;
                    query.ReturnFields.Clear();

                    FilterCompositeType composite = new FilterCompositeType(CutWith.AND);

                    // build terms query: epg_id IN (1, 2, 3 ... bulkSize)
                    var programIds = calculatedPrograms.Skip(skip).Take(bulkSize).Select(program => program.EpgID.ToString());
                    ESTerms terms = new ESTerms(true);
                    terms.Value.AddRange(programIds);
                    composite.AddChild(terms);

                    filter.FilterSettings = composite;
                    query.Filter = filter;

                    string searchQuery = query.ToString();

                    string searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

                    var jsonResult = JObject.Parse(searchResult);

                    JToken tempToken;

                    // check total items - if count is different, something is missing
                    int totalItems = ((tempToken = jsonResult.SelectToken("hits.total")) == null ? 0 : (int)tempToken);

                    if (!(totalItems == programIds.Count()))
                    {
                        isValid = false;
                    }

                    // TODO : explain failures
                }

                if (!isValid)
                {
                    log.Warn($"Missing program from ES index.");
                    throw new Exception("Missing program from ES index");
                }
            });

            result = isValid;

            return result;
        }

        /// <summary>
        /// switch aliases - 
        /// delete epg_203_20190422 for epg_203_20190422_old_bulk_upload_id
        /// add epg_203_20190422 for epg_203_20190422_current_bulk_upload_id
        /// </summary>
        /// <param name="bulkUploadId"></param>
        /// <param name="dateOfIngest"></param>
        private void SwitchAliases(int groupId, long bulkUploadId, DateTime dateOfIngest)
        {
            string dateAlias = GetProgramIndexDateAlias();
            string generalAlias = GetProgramIndexAlias(groupId);

            var previousIndices = _ElasticSearchClient.GetAliases(dateAlias);

            foreach (var index in previousIndices)
            {
                _ElasticSearchClient.RemoveAlias(index, generalAlias);
                _ElasticSearchClient.RemoveAlias(index, dateAlias);
            }

            string newIndex = GetProgramIndexDateName();
            _ElasticSearchClient.AddAlias(newIndex, dateAlias);
            _ElasticSearchClient.AddAlias(newIndex, generalAlias);
        }

        private static IngestProfile GetIngestProfile(int profileId)
        {
            var ingestProfile = IngestProfileManager.GetIngestProfileById(profileId)?.Object;

            if (ingestProfile == null)
            {
                string message = $"Received bulk upload ingest event with invalid ingest profile.";
                log.Error(message);
                throw new Exception(message);
            }

            return ingestProfile;
        }

        private string GetProgramIndexAlias(int groupId)
        {
            return $"{groupId}_epg_v2";
        }

        private string GetProgramIndexDateAlias()
        {
            string dateString = _EventData.DateOfProgramsToIngest.ToString(ESUtils.ES_DATEONLY_FORMAT);
            return $"{_EventData.GroupId}_epg_v2_{dateString}";
        }

        private string GetProgramIndexDateName()
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

        protected virtual double GetTTLMinutes(EpgCB epg)
        {
            return Math.Ceiling((epg.EndDate.AddDays(EXPIRY_DATE_DELTA) - DateTime.UtcNow).TotalMinutes);
        }
    }

}
