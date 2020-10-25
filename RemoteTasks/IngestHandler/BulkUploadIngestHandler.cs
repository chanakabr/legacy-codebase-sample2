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
using Polly;
using Polly.Retry;
using Synchronizer;
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
        private const string REFRESH_INTERVAL_FOR_EMPTY_INDEX = "10s";

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
        
        private List<EpgProgramBulkUploadObject> _CurrentPrograms;
        private List<EpgProgramBulkUploadObject> _PrevDayPrograms;
        private List<EpgProgramBulkUploadObject> _NextDayPrograms;

        private readonly RetryPolicy _IngestRetryPolicy;
        private EpgElasticUpdater _ElasticSearchUpdater;
        private List<EpgProgramBulkUploadObject> _ProgramsToIngest;


        public BulkUploadIngestHandler()
        {
            _ElasticSearchClient = new ElasticSearchApi();
            _CouchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);
            _CurrentPrograms = new List<EpgProgramBulkUploadObject>();
            _PrevDayPrograms = new List<EpgProgramBulkUploadObject>();
            _NextDayPrograms = new List<EpgProgramBulkUploadObject>();

            _IngestRetryPolicy = GetRetrayPolicy<Exception>();
        }

        public async Task Handle(BulkUploadIngestEvent serviceEvent)
        {
            IngestFinalizerConfig ingestFinalizerConfig;
            // Lock all dates before starting the ingest
            var locker = new DistributedLock(serviceEvent.GroupId);
            var epgV2Config = ApplicationConfiguration.Current.EPGIngestV2Configuration;
            var lockNamesPerDate = GetIndexDaysToLock(serviceEvent);

            try
            {
                /*lock days to ingest*/
                //each day locks its previous and next day in case there are overlaps between days
                var isLocked = locker.Lock(lockNamesPerDate.Values,
                    epgV2Config.LockNumOfRetries.Value,
                    epgV2Config.LockRetryIntervalMS.Value,
                    epgV2Config.LockTTLSeconds.Value,
                    $"BulkUpload_{serviceEvent.BulkUploadId}",
                    "ingest_handler");

                //refresh index before starting to work on it ,in order to get the latest data
                foreach (var date in lockNamesPerDate.Keys)
                {
                    IndexManager.ForceRefresh(IndexManager.GetDailyEpgIndexName(serviceEvent.GroupId, date.Date));
                }

                if (!isLocked) { throw new Exception("Failed to aquire lock on ingest dates"); }

                InitilizeHandlerProperties(serviceEvent);
                UnlockEpmtyCrossingDays(serviceEvent, locker);

                var programsToIngestPerChannel = _ProgramsToIngest.GroupBy(p => p.ChannelId).ToDictionary(k => k.Key, v => v.ToList());
                var currentProgramsByChannel = _CurrentPrograms.GroupBy(p => p.ChannelId).ToDictionary(k => k.Key, v => v.ToList());
                var prevProgramsByChannel = _PrevDayPrograms.GroupBy(p => p.ChannelId).ToDictionary(k => k.Key, v => v.ToList());
                var nextProgramsByChannel = _NextDayPrograms.GroupBy(p => p.ChannelId).ToDictionary(k => k.Key, v => v.ToList());

                var overallCrudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
                var nextCrossDaysCrudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
                var prevCrossDaysCrudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();

                _Logger.Info($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}] > starting CRUD calc for channels: [{string.Join(",", programsToIngestPerChannel.Keys)}]");
                foreach (var channelId in programsToIngestPerChannel.Keys)
                {
                    var programsToIngestOfChannel = programsToIngestPerChannel[channelId];
                    if (!currentProgramsByChannel.TryGetValue(channelId, out var currentProgramsOfChannel))
                    {
                        currentProgramsOfChannel = new List<EpgProgramBulkUploadObject>();

                    }

                    if (!prevProgramsByChannel.TryGetValue(channelId, out var prevProgramsOfChannel))
                    {
                        prevProgramsOfChannel = new List<EpgProgramBulkUploadObject>();
                    }

                    if (!nextProgramsByChannel.TryGetValue(channelId, out var nextProgramsOfChannel))
                    {
                        nextProgramsOfChannel = new List<EpgProgramBulkUploadObject>();
                    }

                    var crudOperationsForChannel = HandleIngestForChannel(currentProgramsOfChannel,
                                                                          programsToIngestOfChannel,
                                                                          prevProgramsOfChannel,
                                                                          nextProgramsOfChannel,
                                                                          out var isValid,
                                                                          out var nextCrossDaysCrudOperationsForChannel,
                                                                          out var prevCrossDaysCrudOperationsForChannel);

                    if (!isValid)
                    {
                        _Logger.Info($"BulkId [{_EventData.BulkUploadId}]," +
                            $" Date:[{_EventData.DateOfProgramsToIngest}] > " +
                            $"logical CRUD calculation failed due to invalid programs " +
                            $"according to ingest profile, this day will not be ingested.");
                        return;
                    }

                    overallCrudOperations.AddRangeUnique(crudOperationsForChannel);
                    nextCrossDaysCrudOperations.AddRangeUnique(nextCrossDaysCrudOperationsForChannel);
                    prevCrossDaysCrudOperations.AddRangeUnique(prevCrossDaysCrudOperationsForChannel);
                }

                _Logger.Info($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}] > completed CRUD calculation {overallCrudOperations}");
                var finalEpgSchedule = overallCrudOperations.ItemsToAdd
                        .Concat(overallCrudOperations.ItemsToUpdate)
                        .Concat(overallCrudOperations.AffectedItems)
                        .ToList();


                await UploadEpgImages(finalEpgSchedule);

                var dailyEpgIndexName = IndexManager.GetDailyEpgIndexName(_EventData.GroupId, _EventData.DateOfProgramsToIngest);
                EnsureEpgIndexExistAndSetNoRefresh(dailyEpgIndexName);

                var allAffectedItems =
                    overallCrudOperations.AffectedItems.Concat(nextCrossDaysCrudOperations.AffectedItems)
                    .Concat(prevCrossDaysCrudOperations.AffectedItems);

                if (allAffectedItems.Any())
                {
                    BulkUploadManager.UpdateOrAddBulkUploadAffectedObjects(_BulkUploadObject.Id, allAffectedItems);
                }

                await BulkUploadMethods.UpdateCouchbase(finalEpgSchedule, serviceEvent.GroupId);
                var programsToDelete = overallCrudOperations
                    .ItemsToDelete
                    .Concat(overallCrudOperations.ItemsToUpdate)
                    .Concat(overallCrudOperations.AffectedItems)
                    .ToList();

                var dailyEpgIndex = IndexManager.GetDailyEpgIndexName(serviceEvent.GroupId, serviceEvent.DateOfProgramsToIngest);



                _ElasticSearchUpdater.Update(finalEpgSchedule, programsToDelete, dailyEpgIndex);

                if (nextCrossDaysCrudOperations.AffectedItems.Any() || nextCrossDaysCrudOperations.ItemsToDelete.Any())
                {

                    await BulkUploadMethods.UpdateCouchbase(nextCrossDaysCrudOperations.AffectedItems, serviceEvent.GroupId);


                    programsToDelete = nextCrossDaysCrudOperations
                   .ItemsToDelete
                   .Concat(nextCrossDaysCrudOperations.ItemsToUpdate)
                   .Concat(nextCrossDaysCrudOperations.AffectedItems)
                   .ToList()
                   ;
                    //update ES of next day



                    dailyEpgIndex = IndexManager.GetDailyEpgIndexName(serviceEvent.GroupId, serviceEvent.DateOfProgramsToIngest.AddDays(1));
                    _ElasticSearchUpdater.Update(nextCrossDaysCrudOperations.AffectedItems, programsToDelete, dailyEpgIndex);
                }

                if (prevCrossDaysCrudOperations.AffectedItems.Any()|| prevCrossDaysCrudOperations.ItemsToDelete.Any())
                {
                    await BulkUploadMethods.UpdateCouchbase(prevCrossDaysCrudOperations.AffectedItems, serviceEvent.GroupId);

                    programsToDelete = prevCrossDaysCrudOperations
                   .ItemsToDelete
                   .Concat(prevCrossDaysCrudOperations.ItemsToUpdate)
                   .Concat(prevCrossDaysCrudOperations.AffectedItems)
                   .ToList();
                    //update ES of prev day
                    dailyEpgIndex = IndexManager.GetDailyEpgIndexName(serviceEvent.GroupId, serviceEvent.DateOfProgramsToIngest.AddDays(-1));
                    _ElasticSearchUpdater.Update(prevCrossDaysCrudOperations.AffectedItems, programsToDelete, dailyEpgIndex);
                }

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
                var finalizer = new IngestFinalizer();
                ingestFinalizerConfig = new IngestFinalizerConfig
                {
                    BulkUploadId = serviceEvent.BulkUploadId,
                    GroupId = serviceEvent.GroupId,
                    DateOfProgramsToIngest = serviceEvent.DateOfProgramsToIngest,
                    EPGs = finalEpgSchedule,
                    Languages = _Languages,
                    Results = _ResultsDictionary
                };

                await finalizer.FinalizeEpgIngest(ingestFinalizerConfig);
                _Logger.Info($"BulkUploadId: [{_EventData.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}] > Ingest Handler completed, passing event to Ingest Validation");
            }
            catch (Exception ex)
            {
                _Logger.Error($"An Exception occurred in BulkUploadIngestHandler requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}].", ex);
                try
                {
                    _Logger.Error($"Setting bulk upload results to error status because of an unexpected error, BulkUploadId:[{serviceEvent.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}]", ex);
                    _BulkUploadObject.Results.ForEach(r => r.Status = BulkUploadResultStatus.Error);
                    _BulkUploadObject.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest handler, {ex.Message}");
                    _Logger.Error($"Trying to set fatal status on BulkUploadId:[{serviceEvent.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}].", ex);
                    _BulkUploadObject.AddError(eResponseStatus.Error, $"An unexpected error occored during ingest, {ex.Message}");
                    var result = BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Fatal);
                    _Logger.Error($"An Exception occurred in BulkUploadIngestValidationHandler requestId:[{_EventData.RequestId}], BulkUploadId:[{_EventData.BulkUploadId}], update result status [{result.Status}].", ex);
                }
                catch (Exception innerEx)
                {
                    _Logger.Error($"An Exception occurred when trying to set FATAL status on bulkUpload. requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}] Date:[{_EventData.DateOfProgramsToIngest}].", innerEx);
                    throw;
                }
                throw;
            }
            finally
            {
                ingestFinalizerConfig = null;
                //call the unlock of the ingesthandler partial locks
                locker.Unlock(lockNamesPerDate.Values);
            }

            return;
        }

        private void UnlockEpmtyCrossingDays(BulkUploadIngestEvent serviceEvent, DistributedLock locker)
        {
            //if no crossing dates remove prev day index locks
            if (!_PrevDayPrograms.Any(x => x.StartDate.Date < x.EndDate.Date))
            {
                var dayLock = GetLockName(serviceEvent.GroupId, serviceEvent.DateOfProgramsToIngest.Date.AddDays(-1));
                locker.Unlock(new[] { dayLock });
                _PrevDayPrograms = new List<EpgProgramBulkUploadObject>();
            }

            //if no crossing dates remove next day index locks
            if (!_CurrentPrograms.Any(x => x.StartDate.Date < x.EndDate.Date))
            {
                var dayLock = GetLockName(serviceEvent.GroupId, serviceEvent.DateOfProgramsToIngest.Date.AddDays(1));
                locker.Unlock(new[] { dayLock });
                _NextDayPrograms = new List<EpgProgramBulkUploadObject>();
            }
        }

        private static Dictionary<DateTime, string> GetIndexDaysToLock(BulkUploadIngestEvent serviceEvent)
        {
            var lockdates = new HashSet<DateTime>();
            lockdates.Add(serviceEvent.DateOfProgramsToIngest.Date);//current day          
            lockdates.Add(serviceEvent.DateOfProgramsToIngest.Date.AddDays(1)); //next day
            lockdates.Add(serviceEvent.DateOfProgramsToIngest.Date.AddDays(-1));//prev day
            return lockdates.ToDictionary(x => x, x => GetLockName(serviceEvent.GroupId,x));
        }

        private static string GetLockName(int groupId,DateTime date)
        {
            return $"{groupId}_ingesthandler_lock_{date.ToString("yyyyMMdd")}";
        }


        private void InitilizeHandlerProperties(BulkUploadIngestEvent serviceEvent)
        {
           
            _Logger.Debug($"Starting BulkUploadIngestHandler  requestId:[{serviceEvent.RequestId}], BulkUploadId:[{serviceEvent.BulkUploadId}]");
            _EpgBL = new TvinciEpgBL(serviceEvent.GroupId);
            _EventData = serviceEvent;
            ValidateServiceEvent();
            _BulkUploadObject = BulkUploadMethods.GetBulkUploadData(serviceEvent.GroupId, serviceEvent.BulkUploadId);
            BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, BulkUploadJobStatus.Processing);

            _BulkUploadJobData = _BulkUploadObject.JobData as BulkUploadIngestJobData;
            _IngestProfile = GetIngestProfile();
            _Languages = GetGroupLanguages(out _DefaultLanguage);
            _NonOpcGroupRatios = EpgDal.Get_PicsEpgRatios();
            _GroupRatioNamesToImageTypes = Core.Catalog.CatalogManagement.ImageManager.GetImageTypesMapBySystemName(serviceEvent.GroupId);
            _IsOpc = GroupSettingsManager.IsOpc(serviceEvent.GroupId);
            _CurrentPrograms = new List<EpgProgramBulkUploadObject>();
            _ElasticSearchUpdater = new EpgElasticUpdater(serviceEvent.GroupId, serviceEvent.BulkUploadId, serviceEvent.DateOfProgramsToIngest, _Languages);

            _ResultsDictionary = GetProgramAssetResults();
            _ProgramsToIngest = _ResultsDictionary.Values.SelectMany(r => r.Values).Select(r => r.Object).Cast<EpgProgramBulkUploadObject>().ToList();
            _MinStartDate = _ProgramsToIngest.Min(p => p.StartDate);
            _MaxEndDate = _ProgramsToIngest.Max(p => p.EndDate);

            var epgDailyIndexName = IndexManager.GetDailyEpgIndexName(_EventData.GroupId, _EventData.DateOfProgramsToIngest);
            var isIndexExist = _ElasticSearchClient.IndexExists(epgDailyIndexName);
            List<string> searchIndexes = new List<string>();
            Dictionary<string, List<EpgProgramBulkUploadObject>> programsToGet=new Dictionary<string, List<EpgProgramBulkUploadObject>>();
            if (isIndexExist)
            {
                searchIndexes.Add(epgDailyIndexName);
                programsToGet[epgDailyIndexName] = _CurrentPrograms;
            }
            else
            {
                _Logger.Info($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > no production index exist, returning empty current programs list");
                return;
            }

            ///Get prev and next day programs
            epgDailyIndexName = IndexManager.GetDailyEpgIndexName(_EventData.GroupId, _EventData.DateOfProgramsToIngest.Date.AddDays(-1));
            isIndexExist = _ElasticSearchClient.IndexExists(epgDailyIndexName);
            
            /*get prev days index only if its first day of ingest*/
            if (isIndexExist)
            {
                searchIndexes.Add(epgDailyIndexName);
                programsToGet[epgDailyIndexName] = _PrevDayPrograms;
            }

            epgDailyIndexName = IndexManager.GetDailyEpgIndexName(_EventData.GroupId, _EventData.DateOfProgramsToIngest.Date.AddDays(1));
            isIndexExist = _ElasticSearchClient.IndexExists(epgDailyIndexName);
            
            /*get next days index only if its last day of ingest*/
            if (isIndexExist)
            {
                searchIndexes.Add(epgDailyIndexName);
                programsToGet[epgDailyIndexName] = _NextDayPrograms;
            }

            var allChanels = _ProgramsToIngest.Select(x => x.ChannelId).Distinct().ToList();
            var programsPerIndex = GetCurrentPrograms(programsToGet.Keys, allChanels);

            //get all progarms per index 
            //current prev and next
            foreach (var index in programsPerIndex.Keys)
            {
                if (programsToGet.ContainsKey(index))
                {
                    programsToGet[index].AddRange(programsPerIndex[index]);
                }
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
        }

        private CRUDOperations<EpgProgramBulkUploadObject> HandleIngestForChannel(List<EpgProgramBulkUploadObject> currentPrograms,
            List<EpgProgramBulkUploadObject> programsToIngestOfChannel,
            List<EpgProgramBulkUploadObject> prevProgramsByChannel,
            List<EpgProgramBulkUploadObject> nextProgramsOfChannel,
            out bool isValid,
            out CRUDOperations<EpgProgramBulkUploadObject> nextDaysCrudOperationsForChannel,
            out CRUDOperations<EpgProgramBulkUploadObject> prevDaysCrudOperationsForChannel            
            )
        {
            isValid = true;
            nextDaysCrudOperationsForChannel = new CRUDOperations<EpgProgramBulkUploadObject>();
            prevDaysCrudOperationsForChannel = new CRUDOperations<EpgProgramBulkUploadObject>();
            var overlapsInIngestSource = EpgBL.Utils.GetOverlappingPrograms(programsToIngestOfChannel);
            ValidateSourceInputOverlaps(overlapsInIngestSource);

            var crudOperationsForChannel = CalculateCRUDOperations(currentPrograms, programsToIngestOfChannel);
            SetProgramsWithEpgIds(crudOperationsForChannel);
            var isOverlapValid = CalculateOverlapsByPolicy(crudOperationsForChannel);
            
            var exsitingNewEpg = crudOperationsForChannel.ItemsToAdd.
                Concat(crudOperationsForChannel.ItemsToUpdate).
                Concat(crudOperationsForChannel.RemainingItems).ToList();

            //next days crud
            nextDaysCrudOperationsForChannel = CalculateNextDaysByPolicy(exsitingNewEpg, nextProgramsOfChannel);

            //prev days crud
            prevDaysCrudOperationsForChannel = CalculatePrevDaysByPolicy(exsitingNewEpg, prevProgramsByChannel);

            var maxPrevDay = prevProgramsByChannel.OrderByDescending(x => x.EndDate).FirstOrDefault();
           
            var isGapValid =  CalculateGapsByPolicy(crudOperationsForChannel, maxPrevDay);

            if (!isOverlapValid || !isGapValid)
            {
                _Logger.Debug($"Overlaps or gaps are not valid by ingest profile, bulkId:[{_EventData.BulkUploadId}] date:[{_EventData.DateOfProgramsToIngest}], groupId:[{_BulkUploadObject.GroupId}]");
                // We dont set status here because this day may fail but other days might succeed so we let the bulk calculate new status according to results
                // we decided to allow partial ingest on daily resolution


                var allResults = _ResultsDictionary.Values.SelectMany(x => x.Values).Where(x => x.Errors == null || x.Errors.Length == 0).ToList();
                allResults.ForEach(x => x.AddError(eResponseStatus.Error, "Failed due to other programs errors on same date."));


                UpdateBulkUploadObjectStatusAndResults();
                isValid = false;
                return null;
            }

            AddEpgCBObjects(_ResultsDictionary);
            return crudOperationsForChannel;
        }

        private CRUDOperations<EpgProgramBulkUploadObject> CalculatePrevDaysByPolicy(List<EpgProgramBulkUploadObject> currentDayProgramms, 
            List<EpgProgramBulkUploadObject> prevDayPrograms)
        {

            var crossProgramsCrudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();
            
            
            //get overlaps for progarms that cross date to next day
            switch (_IngestProfile.DefaultOverlapPolicy)
            {
                case eIngestProfileOverlapPolicy.Reject:
                    //TODO need to implement
                    break;
                case eIngestProfileOverlapPolicy.CutSource:
                    //TODO need to implement
                    break;
                case eIngestProfileOverlapPolicy.CutTarget:
                    
                    var fisrtProgramCurrentDay = currentDayProgramms.Where(x => !x.IsAutoFill).OrderBy(x => x.StartDate).FirstOrDefault();                    
                    var allToDel = prevDayPrograms.Where(x => currentDayProgramms.Select(y => y.EpgExternalId).Contains(x.EpgExternalId)).ToList();

                    if (fisrtProgramCurrentDay!=null)
                    {
                        var allProgramsAfterCurrentFirst= prevDayPrograms.Where(x => x.StartDate >= fisrtProgramCurrentDay.StartDate).ToList();
                        allToDel.AddRange(allProgramsAfterCurrentFirst);
                    }

                    if (allToDel.Any())
                    {
                        crossProgramsCrudOperations.ItemsToDelete.AddRange(allToDel);
                    }

                    //should be only one that cross
                    var prevDayProgramsCrossDates = prevDayPrograms.Except(allToDel)
                        .Where(x => x.StartDate.Date != x.EndDate.Date && x.StartDate.Date < x.EndDate.Date).ToList();

                    



                    //find the first overlapping program
                    var prevDayCrossProgram = prevDayProgramsCrossDates.FirstOrDefault();
                    
                    if (prevDayCrossProgram!=null  && currentDayProgramms.Count() > 0)
                    {                        
                        var currentDaysAutoFill = currentDayProgramms.Where(x => x.IsAutoFill);

                        //get first progarm that crossing progarm crosses
                        var firstCurrentProgram = 
                            currentDayProgramms.Except(currentDaysAutoFill)
                            .Where(x => x.StartDate < prevDayCrossProgram.EndDate).OrderBy(x => x.StartDate).FirstOrDefault();

                        if (firstCurrentProgram!=null)
                        {
                            prevDayCrossProgram.EndDate = firstCurrentProgram.StartDate;

                            /*if it shirnks to zero remove it*/
                            TimeSpan dateSpan = (prevDayCrossProgram.EndDate - prevDayCrossProgram.StartDate);
                            if (dateSpan == TimeSpan.Zero)
                            {
                                crossProgramsCrudOperations.ItemsToDelete.Add(prevDayCrossProgram);
                            }
                            else
                            {                                
                                prevDayCrossProgram.EpgCbObjects.ForEach(epg => epg.EndDate = firstCurrentProgram.StartDate);
                                crossProgramsCrudOperations.AffectedItems.Add(prevDayCrossProgram);
                            }
                        }
                    }



                    break;
                default:
                    break;
            }
            return crossProgramsCrudOperations;
        }

        private CRUDOperations<EpgProgramBulkUploadObject> CalculateNextDaysByPolicy(List<EpgProgramBulkUploadObject> exsitingNewEpg,            
            List<EpgProgramBulkUploadObject> nextProgramsOfChannel)
        {
            var crossProgramsCrudOperations = new CRUDOperations<EpgProgramBulkUploadObject>();            
            //should be only one that cross
            

            //get overlaps for progarms that cross date to next day
            switch (_IngestProfile.DefaultOverlapPolicy)
            {
                case eIngestProfileOverlapPolicy.Reject:
                    break;
                case eIngestProfileOverlapPolicy.CutSource:
                    break;
                case eIngestProfileOverlapPolicy.CutTarget:
                    var sameIds = exsitingNewEpg.Select(x => x.EpgExternalId)
                            .Intersect(nextProgramsOfChannel.Select(x => x.EpgExternalId));

                    var allCrossingProgsToDelete = nextProgramsOfChannel
                        .Where(x =>  sameIds.Contains(x.EpgExternalId)).ToList();

                    var newProgramsCrossDates =
                        exsitingNewEpg.Where(x => x.StartDate.Date != x.EndDate.Date && x.StartDate.Date < x.EndDate.Date).ToList();

                    var crossProg = newProgramsCrossDates.FirstOrDefault();

                    if (crossProg != null)
                    {

                        allCrossingProgsToDelete.AddRange(nextProgramsOfChannel.Where(x => x.EndDate <= crossProg.EndDate || sameIds.Contains(x.EpgExternalId)).Distinct().ToList());

                        var programToUpdate = nextProgramsOfChannel
                            .Except(allCrossingProgsToDelete)
                            .FirstOrDefault(x => x.StartDate < crossProg.EndDate && x.EndDate > crossProg.EndDate);

                        if (programToUpdate != null)
                        {
                            programToUpdate.StartDate = crossProg.EndDate;

                            /*if it shirnk to zero remove it*/
                            TimeSpan dateSpan = (programToUpdate.EndDate - programToUpdate.StartDate);
                            if (dateSpan == TimeSpan.Zero)
                            {
                                crossProgramsCrudOperations.ItemsToDelete.Add(programToUpdate);
                            }
                            else
                            {
                                programToUpdate.EpgCbObjects.ForEach(epg => epg.StartDate = crossProg.EndDate);
                                crossProgramsCrudOperations.AffectedItems.Add(programToUpdate);
                            }
                        }
                    }
                    crossProgramsCrudOperations.ItemsToDelete.AddRange(allCrossingProgsToDelete.Distinct());


                    break;
                default:
                    break;
            }

            return crossProgramsCrudOperations;
        }

        private void UpdateBulkUploadObjectStatusAndResults(BulkUploadJobStatus? statusToSet = null)
        {
            var resultsToUpdate = _ResultsDictionary.Values.SelectMany(r => r.Values).ToList();
            BulkUploadManager.UpdateBulkUploadResults(resultsToUpdate, out var jobStatusByResultStatus);
            var jobStatus = statusToSet ?? jobStatusByResultStatus;

            _Logger.Debug($"UpdateBulkUploadObjectStatusAndResults > updated results, calculated status by results: [{jobStatusByResultStatus}], requested status to set:[{statusToSet}], setting status:[{jobStatus}]");
            BulkUploadManager.UpdateBulkUploadStatusWithVersionCheck(_BulkUploadObject, jobStatus);
        }

        private bool CalculateGapsByPolicy(CRUDOperations<EpgProgramBulkUploadObject> crudOperations, EpgProgramBulkUploadObject maxPrevDay)
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
                .Where(c=>!c.IsAutoFill)
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
                    

                    var firstProgram = newEpgSchedule.OrderBy(x => x.StartDate).First(); 
                    var lastProgram = newEpgSchedule.OrderByDescending(x => x.EndDate).First();

                    var startOfDay =firstProgram.StartDate.StartOfDay();
                    
                    //if prev day crossing to current day
                    if (maxPrevDay!=null && maxPrevDay.EndDate.Date== firstProgram.StartDate.Date)
                    {
                        startOfDay = maxPrevDay.EndDate;
                    }

                    var endOfDay=firstProgram.StartDate.EndOfDay();

                    //if this day is smaller than the first program fill gap between start of day to first program
                    if (startOfDay < firstProgram.StartDate)
                    {
                        var warnMessage = $"Autofilling gap between start of day to first day's program {firstProgram.EpgExternalId}, {startOfDay} to  {firstProgram.StartDate}";                        
                        TryAddWarnning(firstProgram.ChannelId, firstProgram.EpgExternalId, eResponseStatus.EPGSProgramDatesError, warnMessage);                        
                        var autofillProgram = GetDefaultAutoFillProgram(startOfDay, 
                                                                        firstProgram.StartDate,
                                                                        firstProgram.ChannelId, 
                                                                        firstProgram.ChannelExternalId, 
                                                                        firstProgram.LinearMediaId);
                        autofillEpgs.Add(autofillProgram);
                    }

                    //fill gaps between last program to end of day only if it ends on same day.
                    if ( lastProgram.EndDate < endOfDay)
                    {
                        var warnMessage = $"Autofilling gap between last day's program {lastProgram.EpgExternalId} to end of day,  {lastProgram.EndDate} to {endOfDay} ";                        
                        TryAddWarnning(lastProgram.ChannelId, lastProgram.EpgExternalId, eResponseStatus.EPGSProgramDatesError, warnMessage);
                        var autofillProgram = GetDefaultAutoFillProgram(lastProgram.EndDate,
                                                                        endOfDay,
                                                                        lastProgram.ChannelId,
                                                                        lastProgram.ChannelExternalId,
                                                                        lastProgram.LinearMediaId);
                        autofillEpgs.Add(autofillProgram);
                    }

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

                foreach (var prog in autofillEpgs)
                {                    
                    prog.EpgCbObjects = new List<EpgCB>();                    
                    var epgItems = GetAutoFillEpgCBDocuments(prog);
                    prog.EpgCbObjects.AddRange(epgItems.Values);
                }

                crudOperations.ItemsToAdd.AddRange(autofillEpgs);
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
            var overlaps = EpgBL.Utils.GetOverlappingPrograms(allNew, exsitingNewEpg);

            foreach (var overlappingProgs in overlaps)
            {
                var programToIngest = overlappingProgs.Item1;
                var currentProgram = overlappingProgs.Item2;

                var msg = $"Program [{programToIngest.EpgExternalId}] overlapping [{currentProgram.EpgExternalId}";

                switch (_IngestProfile.DefaultOverlapPolicy)
                {
                    case eIngestProfileOverlapPolicy.Reject:
                        msg += $", policy is set to Reject, rejecting input on overlapping items";
                        _ResultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddError(eResponseStatus.Error, msg);
                        isValid = false;
                        break;
                    case eIngestProfileOverlapPolicy.CutSource:

                        //if program to ingest starts before current program
                        if (programToIngest.StartDate < currentProgram.StartDate && 
                            programToIngest.EndDate>=currentProgram.StartDate)
                        {
                            //set end date to current programs strat date
                            msg += $", policy is set to CutSource, Changing new program to ingest end date from [{programToIngest.EndDate}], to [{currentProgram.StartDate}]";
                            programToIngest.EndDate = currentProgram.StartDate;
                        }
                        //program to ingest starts in the middle of current program and
                        // ends after
                        else if (programToIngest.StartDate>=currentProgram.StartDate &&
                            programToIngest.EndDate>currentProgram.EndDate)
                        {
                            msg += $", policy is set to CutSource, Changing new program to ingest start date from [{programToIngest.StartDate}], to [{currentProgram.EndDate}]";
                            programToIngest.StartDate = currentProgram.EndDate;
                        }
                        else
                        {
                            //cannot ingest this program it should be removed.
                            msg += $", policy is set to CutSource, program {programToIngest.EpgExternalId} will be removed as its end and start date are inside an existing program.";
                            crudOperations.ItemsToAdd.Remove(programToIngest);
                            crudOperations.ItemsToUpdate.Remove(programToIngest);
                            _ResultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].Status = BulkUploadResultStatus.Ok;
                        }

                        _ResultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
                        break;
                    case eIngestProfileOverlapPolicy.CutTarget:

                        //if program to ingest starts before current program
                        if (currentProgram.StartDate < programToIngest.StartDate &&
                            currentProgram.EndDate >= programToIngest.StartDate)
                        {
                            //set end date to current programs strat date
                            msg += $", policy is set to CutTarget, Changing current program end date from [{currentProgram.EndDate}], to [{programToIngest.StartDate}]";
                            currentProgram.EndDate = programToIngest.StartDate;
                            currentProgram.EpgCbObjects.ForEach(epg => epg.EndDate = programToIngest.StartDate);
                        }
                        //program to ingest starts in the middle of current program and
                        // ends after
                        else if (currentProgram.StartDate >= programToIngest.StartDate &&
                            currentProgram.EndDate >= programToIngest.EndDate)
                        {
                            msg += $", policy is set to CutTarget, Changing current program start date from [{currentProgram.StartDate}], to [{programToIngest.EndDate}]";
                            currentProgram.StartDate = programToIngest.EndDate;
                            currentProgram.EpgCbObjects.ForEach(epg => epg.StartDate = programToIngest.EndDate);
                        }
                        
                        crudOperations.AffectedItems.Add(currentProgram);
                        var data = crudOperations.AffectedItems.ToHashSet();
                        crudOperations.AffectedItems.Clear();
                        crudOperations.AffectedItems.AddRange(data);
                        crudOperations.RemainingItems.Remove(currentProgram);

                        _ResultsDictionary[programToIngest.ChannelId][programToIngest.EpgExternalId].AddWarning((int)eResponseStatus.EPGProgramOverlapFixed, msg);
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

        private Dictionary<string, List<EpgProgramBulkUploadObject>> GetCurrentPrograms(IEnumerable<string> indexNames, List<int> allChanels)
        {
            _Logger.Debug($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] GetCurrentProgramsByDate > indexName:[{indexNames}]");
            var data = new Dictionary<string, List<EpgProgramBulkUploadObject>>();
            ESTerms term = ESTerms.GetSimpleNumericTerm<int>("epg_channel_id", allChanels);
            ESQuery query = new ESQuery(term);
            query.Fields = new List<string>() { "_index", "document_id", "epg_id", "start_date", "end_date", "epg_identifier", "is_auto_fill", "epg_channel_id" };
            var maxResultsSize = ApplicationConfiguration.Current.ElasticSearchConfiguration.MaxResults.Value;
            query.Size = maxResultsSize <= 0 ? 10000 : maxResultsSize;

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _ElasticSearchClient.Search(string.Join(",", indexNames), EpgElasticUpdater.DEFAULT_INDEX_MAPPING_TYPE, ref searchQuery);

            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                var json = JObject.Parse(searchResult);

                var hits = (json["hits"]["hits"] as JArray);


                foreach (var hit in hits)
                {
                    var index = ESUtils.ExtractValueFromToken<string>(hit, "_index");
                    if (!data.ContainsKey(index))
                    {
                        data[index] = new List<EpgProgramBulkUploadObject>();
                    }

                    var hitFields = hit["fields"];
                    var epgItem = new EpgProgramBulkUploadObject();
                    epgItem.EpgExternalId = ESUtils.ExtractValueFromToken<string>(hitFields, "epg_identifier");
                    epgItem.StartDate = ESUtils.ExtractDateFromToken(hit["fields"], "start_date");
                    epgItem.EndDate = ESUtils.ExtractDateFromToken(hit["fields"], "end_date");
                    epgItem.EpgId = ESUtils.ExtractValueFromToken<ulong>(hitFields, "epg_id");
                    epgItem.IsAutoFill = ESUtils.ExtractValueFromToken<bool>(hitFields, "is_auto_fill");
                    epgItem.ChannelId = ESUtils.ExtractValueFromToken<int>(hitFields, "epg_channel_id");
                    epgItem.EpgCbObjects = new List<EpgCB>();
                    data[index].Add(epgItem);
                }
            }

            var result = data.Values.SelectMany(x => x);            
            var documentIds = _EpgBL.GetEpgsCBKeys(_EventData.GroupId, result.Select(x => (long)x.EpgId), _Languages.Values, false);
            List<EpgCB> epgCbList = EpgDal.GetEpgCBList(documentIds);
            //fill all cb objects 
            foreach (var epg in result)
            {
                epg.EpgCbObjects.AddRange(epgCbList.Where(x => x.EpgID == epg.EpgId));
            }
            return data;
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

        private void EnsureEpgIndexExistAndSetNoRefresh(string dailyEpgIndexName)
        {
            
            var isIndexExist = _ElasticSearchClient.IndexExists(dailyEpgIndexName);
            if (!isIndexExist)
            {
                // todo: retry index creation if fails ? 
                _Logger.Info($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > no production index exist, creating new one with name [{dailyEpgIndexName}]");

                var isIndexCreated = BuildNewIndex(dailyEpgIndexName);
                if (!isIndexCreated)
                {
                    _Logger.Error($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > index creation failed [{dailyEpgIndexName}]");
                    throw new Exception($"index creation failed");
                }

                var epgIndexAlias = IndexManager.GetEpgIndexAlias(_EventData.GroupId);
                _IngestRetryPolicy.Execute(() =>
                {
                    var isGloablAliasAdded = _ElasticSearchClient.AddAlias(dailyEpgIndexName, epgIndexAlias);
                    if (!isGloablAliasAdded)
                    {
                        _Logger.Error($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > index set alias failed [{dailyEpgIndexName}], alias [{epgIndexAlias}]");
                        throw new Exception($"index set alias failed");
                    }
                });

            }

            // shut down refhresh of index while bulk uploading
            _IngestRetryPolicy.Execute(() =>
            {
                var isSetRefreshSuccess = _ElasticSearchClient.UpdateIndexRefreshInterval(dailyEpgIndexName, "-1");
                if (!isSetRefreshSuccess)
                {
                    _Logger.Error($"BulkId [{_EventData.BulkUploadId}], Date:[{_EventData.DateOfProgramsToIngest}] > index set refresh to -1 failed [{isSetRefreshSuccess}], dailyEpgIndexName [{dailyEpgIndexName}]");
                    throw new Exception("Could not set index refresh interval");
                }
            });
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

                
                
                _IngestRetryPolicy.Execute(() =>
                {
                    IndexManager.CreateNewEpgIndex(_EventData.GroupId,
                        catalogGroupCache,
                        group,
                        _Languages.Values,
                        _DefaultLanguage,
                        newIndexName,
                        isRecording: false,
                        shouldBuildWithReplicas: true,
                        shouldUseNumOfConfiguredShards: true,
                        refreshInterval: REFRESH_INTERVAL_FOR_EMPTY_INDEX);
                });
            }
            catch (Exception e)
            {
                _Logger.Error("Error while building new index. ", e);
                _BulkUploadObject.AddError(eResponseStatus.Error, "Error while building new index. ");
                return false;
            }

            return true;
        }

        private static RetryPolicy GetRetrayPolicy<TException>(int retryCount = 3) where TException : Exception
        {
            return Policy.Handle<TException>()
            .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time, attempt, ctx) =>
            {
                _Logger.Warn($"upsert attemp [{attempt}/{retryCount}] Failed, waiting for:[{time.TotalSeconds}] seconds.", ex);
            });
        }

        private Dictionary<string, EpgCB> GetAutoFillEpgCBDocuments(EpgProgramBulkUploadObject prog)
        {
            var key = $"autofill_{_BulkUploadObject.GroupId}";
            _AutoFillEpgsCB = _AutoFillEpgsCB ?? _CouchbaseManager.Get<Dictionary<string, EpgCB>>(key, true);

            if (_AutoFillEpgsCB == null)
            {
                string message = $"Could not find default auto fill document under key: {key}";
                _Logger.Error(message);
                throw new Exception(message);
            }

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
                autofillDocs[doc.Key].CreateDate = DateTime.UtcNow;
                autofillDocs[doc.Key].UpdateDate = DateTime.UtcNow;
            }
            return autofillDocs;
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