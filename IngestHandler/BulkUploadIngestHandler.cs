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

namespace IngestHandler
{
    public class BulkUploadIngestHandler : IServiceEventHandler<BulkUploadIngestEvent>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string DEFAULT_INDEX_TYPE = "epg";
        private static readonly string EPG_SEQUENCE_DOCUMENT = "epg_sequence_document";
        private static readonly double EXPIRY_DATE = (ApplicationConfiguration.EPGDocumentExpiry.IntValue > 0) ? ApplicationConfiguration.EPGDocumentExpiry.IntValue : 7;

        private ElasticSearchApi _ElasticSearchClient = null;
        private CouchbaseManager.CouchbaseManager _CouchbaseManager = null;

        public BulkUploadIngestHandler()
        {
            _ElasticSearchClient = new ElasticSearchApi();
            _CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);
        }

        public Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            try
            {
                log.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
                var groupId = serviceEvent.GroupId;
                var bulkUploadId = serviceEvent.BulkUploadId;
                var bulkUploadData = GetBulkUploadData(serviceEvent, bulkUploadId);

                ValidateServiceEvent(serviceEvent);

                var results = GetProgramAssetResults(serviceEvent, bulkUploadData);
                AddEpgCBObjects(groupId, results);
                var epgBulkUploadObjects = results.Values.Select(r => r.Object).Cast<EpgProgramBulkUploadObject>();

                var minStartDate = epgBulkUploadObjects.Min(p => p.StartDate);
                var maxEndDate = epgBulkUploadObjects.Max(p => p.EndDate);
                var currentPrograms = GetCurrentProgramsByDate(groupId, minStartDate, maxEndDate);
                var programsToIngest = epgBulkUploadObjects.SelectMany(p=>p.EpgCbObjects).ToList();

                // TODO: elastic, type is used to identify languages .. 
                // coulnd find document_id in elastic response
                // 
                var crudOperations = CalculateCRUDOperations(groupId, currentPrograms, programsToIngest);
                var finalEpgState = CalculateSimulatedFinalStateAfterIngest(crudOperations.ItemsToAdd, crudOperations.ItemsToUpdate);


            }
            catch (Exception ex)
            {
                log.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}].", ex);
                return Task.FromException(ex);
            }

            return Task.CompletedTask;
        }

        private void AddEpgCBObjects(int groupId, Dictionary<string, BulkUploadProgramAssetResult> results)
        {
            // TODO: move language getting to the right place
            var languages = GroupLanguageManager.GetGroupLanguages(groupId);
            var defaultLanguage = languages.FirstOrDefault(l => l.IsDefault);
            if (defaultLanguage == null) { throw new Exception($"No main language defined for group:[{groupId}], ingest failed"); }

            foreach (var progResult in results.Values)
            {
                var programBulkUploadObject = progResult.Object as EpgProgramBulkUploadObject;
                programBulkUploadObject.EpgCbObjects = new List<EpgCB>();
                foreach (var lang in languages)
                {
                    var epgItem = GetEpgCBObject(lang.Code, defaultLanguage.Code, programBulkUploadObject, progResult);
                    programBulkUploadObject.EpgCbObjects.Add(epgItem);
                }
            }
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

        private void ValidateServiceEvent(BulkUploadIngestEvent serviceEvent)
        {

            if (serviceEvent.ProgramsToIngest?.Any() != true)
            {
                throw new Exception($"Received bulk upload ingest event with null or empty programs to insert. group id = {serviceEvent.GroupId} id = {serviceEvent.BulkUploadId}");
            }

            if (serviceEvent.ProgramsToIngest.Count() == 0)
            {
                log.Warn($"Received bulk upload ingest event with 0 programs to insert. group id ={serviceEvent.GroupId} id = {serviceEvent.BulkUploadId}");
            }

            //var groupId = serviceEvent.GroupId;
            //var bulkUploadId = serviceEvent.BulkUploadId;
            //var bulkUploadData = GetBulkUploadData(serviceEvent, bulkUploadId);


            //var results = GetProgramAssetResults(serviceEvent, bulkUploadData);
            //var programsToIngest = ParseAssetResultsToEpgCB(results);


            //List<EpgCB> programsToAdd;
            //List<EpgCB> programsToUpdate;
            //List<EpgCB> programsToDelete;

            //CalculateCRUDOperations(groupId, currentPrograms, programsToIngest, out programsToAdd, out programsToUpdate, out programsToDelete);

            //List<EpgCB> calculatedPrograms = CalculateSimulatedFinalStateAfterIngest(programsToAdd, programsToUpdate);

            //bool isValid = ValidateProgramDates(calculatedPrograms, programAssetResultsDictionary, ingestProfile);

            //if (!isValid)
            //{
            //    bulkUploadData.SetStatus(ApiObjects.Response.eResponseStatus.Fail, "Ingest handler failure");
            //}
            //else
            //{
            //    // ?
            //    // ?
            //    // ?
            //    // TODO : UPLOAD PICTURES
            //    // ?
            //    // ?
            //    // ?

            //    InsertIngestedProgramsToCouchbase(groupId, calculatedPrograms, bulkUploadId);

            //    /*
            //     *  real name: epg_203_20190422_123456
            //        alias: epg_203_20190422

            //        reindex epg_203_20190422 to epg_203_20190422_current_bulk_upload_id

            //        update cloned index
            //     */

            //    // duplicate the index of this day
            //    var dateOfIngest = serviceEvent.DateOfProgramsToIngest;
            //    bool cloneResult = CloneExistingIndex(groupId, bulkUploadId, dateOfIngest);

            //    if (cloneResult)
            //    {
            //        bulkUploadData.SetStatus(ApiObjects.Response.eResponseStatus.OK, "Ingest handler success");
            //    }
            //    else
            //    {
            //        UpdateClonedIndex(groupId, bulkUploadId, dateOfIngest, calculatedPrograms, programsToDelete);

            //        bool isClonedIndexValid = ValidateClonedIndex(groupId, bulkUploadId, dateOfIngest, calculatedPrograms);

            //        if (isClonedIndexValid)
            //        {
            //            SwitchAliases(groupId, bulkUploadId, dateOfIngest);

            //            // update all INGESTED bulk upload results to status success

            //            bulkUploadData.SetStatus(ApiObjects.Response.eResponseStatus.OK, "Ingest handler success");
            //        }
            //        else
            //        {
            //            bulkUploadData.SetStatus(ApiObjects.Response.eResponseStatus.Fail, "Ingest handler failure");
            //        }
            //    }
            //}

            //// update bulk upload in the end
            //BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(bulkUploadData, BulkUploadJobStatus.Success);
        }

        private Dictionary<string, BulkUploadProgramAssetResult> GetProgramAssetResults(BulkUploadIngestEvent serviceEvent, BulkUpload bulkUploadData)
        {
            var programsToIngest = serviceEvent.ProgramsToIngest;
            var bulkUploadResultsDictionary = bulkUploadData.Results.Cast<BulkUploadProgramAssetResult>().ToDictionary(k => k.ProgramExternalId);

            var programAssetResultsDictionary = bulkUploadData.Results.Cast<BulkUploadProgramAssetResult>().ToDictionary(program => program.ProgramExternalId);
            var ingestProfile = GetIngestProfile(bulkUploadData);

            foreach (var programToIngest in programsToIngest)
            {
                programAssetResultsDictionary[programToIngest.ParsedProgramObject.external_id].Object = programToIngest;
            }

            return programAssetResultsDictionary;
        }
        
        private static BulkUpload GetBulkUploadData(BulkUploadIngestEvent serviceEvent, long bulkUploadId)
        {
            var bulkUploadData = BulkUploadManager.GetBulkUpload(serviceEvent.GroupId, bulkUploadId);

            if (bulkUploadData?.Object == null)
            {
                string message = string.Empty;

                if (bulkUploadData != null && bulkUploadData.Status != null)
                {
                    message = bulkUploadData.Status.Message;
                }

                throw new Exception($"Received invalid bulk upload. group id ={serviceEvent.GroupId} id = {serviceEvent.BulkUploadId} message = {message}");
            }

            return bulkUploadData.Object;
        }

        private List<EpgCB> GetCurrentProgramsByDate(int groupId, DateTime minStartDate, DateTime maxEndDate)
        {
            List<EpgCB> result = new List<EpgCB>();
            string index = GetProgramIndexAlias(groupId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_ElasticSearchClient.IndexExists(index))
            {
                return result;
            }

            string type = DEFAULT_INDEX_TYPE;

            FilteredQuery query = new FilteredQuery(true);

            // Program end date > minimum start date
            ESRange minimumRange = new ESRange(false)
            {
                Key = "end_date"
            };
            minimumRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.GTE, minStartDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT)));

            // program start date < maximum end date
            ESRange maximumRange = new ESRange(false)
            {
                Key = "start_date"
            };
            maximumRange.Value.Add(new KeyValuePair<eRangeComp, string>(eRangeComp.LTE, maxEndDate.ToString(ElasticSearch.Common.Utils.ES_DATE_FORMAT)));

            FilterCompositeType filterCompositeType = new FilterCompositeType(ApiObjects.SearchObjects.CutWith.AND);
            filterCompositeType.AddChild(minimumRange);
            filterCompositeType.AddChild(maximumRange);

            query.Filter = new QueryFilter()
            {
                FilterSettings = filterCompositeType
            };

            query.ReturnFields.Clear();
            query.ReturnFields.Add("document_id");
            query.ReturnFields.Add("epg_id");
            query.ReturnFields.Add("start_date");
            query.ReturnFields.Add("end_date");

            // get the epg document ids from elasticsearch
            string searchQuery = query.ToString();
            var searchResult = _ElasticSearchClient.Search(index, type, ref searchQuery);

            //
            // TODO : get epg complete data from CB
            // LANGUAGE
            //
            List<string> documentIds = new List<string>();

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                JObject json = JObject.Parse(searchResult);

                var hits = (json["hits"] as JArray);

                foreach (var hit in hits)
                {
                    documentIds.Add(hit["document_id"].ToString());
                }

                result = new EpgDal_Couchbase(groupId).GetProgram(documentIds);
            }

            return result;
        }

        private CRUDOperations<EpgCB> CalculateCRUDOperations(int groupId, IList<EpgCB> currentPrograms, IList<EpgCB> programsToIngest)
        {
            var crudOperations = new CRUDOperations<EpgCB>
            {
                ItemsToAdd = new List<EpgCB>(),
                ItemsToUpdate = new List<EpgCB>(),
                ItemsToDelete = new List<EpgCB>(),
            };

            var currentProgramsDictionary = currentPrograms.ToDictionary(epg => epg.EpgIdentifier);
            var programsToIngestDictionary = programsToIngest.ToDictionary(epg => epg.EpgIdentifier);

            foreach (var programToIngest in programsToIngestDictionary)
            {
                // if a program exists both on newly ingested epgs and in index - it's an update
                if (currentProgramsDictionary.ContainsKey(programToIngest.Key))
                {
                    // update the epg id of the ingested programs with their existing epg id from CB
                    programToIngest.Value.EpgID = currentProgramsDictionary[programToIngest.Key].EpgID;
                    crudOperations.ItemsToUpdate.Add(programToIngest.Value);
                }
                else
                {
                    // if it exists only on newly ingested epgs and not in index, it's a program to add
                    crudOperations.ItemsToAdd.Add(programToIngest.Value);
                }
            }

            foreach (var currentProgram in currentProgramsDictionary)
            {
                // if a program exists in index but not in newly ingested programs, it should be deleted
                if (!programsToIngestDictionary.ContainsKey(currentProgram.Key))
                {
                    crudOperations.ItemsToDelete.Add(currentProgram.Value);
                }
            }

            return crudOperations;
        }

        private List<EpgCB> CalculateSimulatedFinalStateAfterIngest(IList<EpgCB> programsToAdd, IList<EpgCB> programsToUpdate)
        {
            List<EpgCB> result = new List<EpgCB>();

            // set the new programs with new IDs from sequence document in couchbase
            foreach (var program in programsToAdd)
            {
                
                var newEPGId = _CouchbaseManager.Increment(EPG_SEQUENCE_DOCUMENT, 1);
                program.EpgID = newEPGId + 1000000;
            }

            // union lists and order by start date
            result.Union(programsToAdd);
            result.Union(programsToUpdate);
            result = result.OrderBy(program => program.StartDate).ToList();

            return result;
        }

        private bool ValidateProgramDates(List<EpgCB> calculatedPrograms, Dictionary<string, BulkUploadProgramAssetResult> programAssetResultsDictionary, IngestProfile ingestProfile)
        {
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
        private void InsertIngestedProgramsToCouchbase(int groupId, List<EpgCB> calculatedPrograms, long bulkUploadId)
        {
            var dal = new EpgDal_Couchbase(groupId);

            foreach (var program in calculatedPrograms)
            {
                // TODO: CONSIDER LANGUAGE - is it enough?
                string key = $"{program.EpgID}__{program.Language}_{bulkUploadId}";
                program.DocumentId = key;

                bool insertResult = dal.InsertProgram(key, program, program.EndDate.AddDays(EXPIRY_DATE));

                if (!insertResult)
                {
                    log.Error($"Failed inserting program. group {groupId} epgId {program.EpgID} bulkUploadId {bulkUploadId}");
                }
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
        private bool CloneExistingIndex(int groupId, long bulkUploadId, DateTime dateOfIngest)
        {
            string source = GetProgramIndexDateAlias(groupId, dateOfIngest);
            string destination = GetProgramIndexDateName(groupId, dateOfIngest, bulkUploadId);
            // TODO: check if reindex works with alias. if not, get the original index name and reindex it
            bool result = _ElasticSearchClient.Reindex(source, destination);

            if (result)
            {
                log.Debug($"Reindex {source} to {destination} success");
            }
            else
            {
                log.ErrorFormat($"Reindex {source} to {destination} failure");
            }

            return result;
        }

        private void UpdateClonedIndex(int groupId, long bulkUploadId, DateTime dateOfIngest, List<EpgCB> calculatedPrograms, List<EpgCB> programsToDelete)
        {
            // basic variables initialization
            int bulkSize = ApplicationConfiguration.ElasticSearchHandlerConfiguration.BulkSize.IntValue;
            string index = GetProgramIndexDateName(groupId, dateOfIngest, bulkUploadId);
            List<ESBulkRequestObj<string>> bulkRequests = new List<ESBulkRequestObj<string>>();
            ESSerializerV2 serializer = new ESSerializerV2();
            bool isOpc = GroupSettingsManager.IsOpc(groupId);
            HashSet<string> metasToPad = GetMetasToPad(groupId);

            foreach (var program in calculatedPrograms)
            {
                program.PadMetas(metasToPad);

                //
                // TODO: this should already happen in deserialize???
                // 
                //// used only to currently support linear media id search on elastic search
                //if (isOpc && linearChannelSettings.ContainsKey(epg.ChannelID.ToString()))
                //{
                //    epg.LinearMediaId = linearChannelSettings[epg.ChannelID.ToString()].linearMediaId;
                //}

                string suffix = program.Language;
                LanguageObj language = null;

                // Serialize EPG object to string
                string serializedEpg = serializer.SerializeEpgObject(program, suffix, isOpc);
                string epgType = GetTanslationType(DEFAULT_INDEX_TYPE, language);

                double totalMinutes = GetTTLMinutes(program);
                string ttl = string.Format("{0}m", totalMinutes);

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
                            log.Error($"Could not add EPG to ES index. GroupID={groupId} epgId={item.Key} error={item.Value}");
                        }
                    }

                    bulkRequests.Clear();
                }
            }

            // If we have anything left that is less than the size of the bulk
            if (bulkRequests.Count > 0)
            {
                var invalidResults = _ElasticSearchClient.CreateBulkRequest(bulkRequests);

                if (invalidResults != null && invalidResults.Count > 0)
                {
                    foreach (var item in invalidResults)
                    {
                        log.Error($"Could not add EPG to ES index. GroupID={groupId} epgId={item.Key} error={item.Value}");
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

            string index = GetProgramIndexDateName(groupId, dateOfIngest, bulkUploadId);

            // TODO: LANGUAGEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
            string type = DEFAULT_INDEX_TYPE;
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
            string dateAlias = GetProgramIndexDateAlias(groupId, dateOfIngest);
            string generalAlias = GetProgramIndexAlias(groupId);

            var previousIndices = _ElasticSearchClient.GetAliases(dateAlias);

            foreach (var index in previousIndices)
            {
                _ElasticSearchClient.RemoveAlias(index, generalAlias);
                _ElasticSearchClient.RemoveAlias(index, dateAlias);
            }

            string newIndex = GetProgramIndexDateName(groupId, dateOfIngest, bulkUploadId);
            _ElasticSearchClient.AddAlias(newIndex, dateAlias);
            _ElasticSearchClient.AddAlias(newIndex, generalAlias);
        }

        private static IngestProfile GetIngestProfile(BulkUpload bulkUploadData)
        {
            IngestProfile ingestProfile = null;
            if (bulkUploadData?.JobData is BulkUploadIngestJobData ingestJobData)
            {
                var ingestProfileId = ingestJobData.IngestProfileId;
                ingestProfile = IngestProfileManager.GetIngestProfileById(ingestProfileId)?.Object;
            }

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

        private string GetProgramIndexDateAlias(int groupId, DateTime date)
        {
            string dateString = date.ToString(Utils.ES_DATEONLY_FORMAT);
            return $"{groupId}_epg_v2_{dateString}";
        }

        private string GetProgramIndexDateName(int groupId, DateTime date, long bulkUploadId)
        {
            string dateString = date.ToString(Utils.ES_DATEONLY_FORMAT);
            return $"{groupId}_epg_v2_{dateString}_{bulkUploadId}";
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
            return Math.Ceiling((epg.EndDate.AddDays(EXPIRY_DATE) - DateTime.UtcNow).TotalMinutes);
        }
    }

}
