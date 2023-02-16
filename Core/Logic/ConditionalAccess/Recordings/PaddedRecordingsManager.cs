using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ApiLogic.kronos;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Recordings;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using CachingProvider.LayeredCache;
using DAL;
using DAL.Recordings;
using Newtonsoft.Json;
using Notifiers;
using Phoenix.Generated.Tasks.Scheduled.EvictRecording;
using Phoenix.Generated.Tasks.Scheduled.RetryRecording;
using Phoenix.Generated.Tasks.Scheduled.VerifyRecordingFinalStatus;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using Synchronizer;
using Tvinci.Core.DAL;
using TVinciShared;
using Status = ApiObjects.Response.Status;

namespace Core.Recordings
{
    public class PaddedRecordingsManager
    {
        private const string SCHEDULED_TASKS_ROUTING_KEY = "PROCESS_RECORDING_TASK\\{0}";
        private const int MaxAllowedActiveRecordings = 2;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PaddedRecordingsManager> LazyInstance = new Lazy<PaddedRecordingsManager>(
            () => new PaddedRecordingsManager(RecordingsRepository.Instance), LazyThreadSafetyMode.PublicationOnly);

        public static PaddedRecordingsManager Instance => LazyInstance.Value;
        private readonly IRecordingsRepository _repository;
        private Synchronizer.CouchbaseSynchronizer synchronizer = null;
        private readonly int RECOVERY_GRACE_PERIOD = 864000;
        private readonly int MINUTES_ALLOWED_DIFFERENCE = 5;
        private readonly int MINUTES_RETRY_INTERVAL;
        private readonly int MAXIMUM_RETRIES_ALLOWED;

        public PaddedRecordingsManager(IRecordingsRepository repository)
        {
            _repository = repository;
            synchronizer = new CouchbaseSynchronizer(1000, 60);
            synchronizer.SynchronizedAct += synchronizer_SynchronizedAct;

            int retryInterval = ApplicationConfiguration.Current.CDVRAdapterConfiguration.RetryInterval.Value;
            int maximumRetries = ApplicationConfiguration.Current.CDVRAdapterConfiguration.MaximumRetriesAllowed.Value;

            if (retryInterval != 0)
            {
                MINUTES_RETRY_INTERVAL = retryInterval;
            }
            else
            {
                MINUTES_RETRY_INTERVAL = 5;
            }

            if (maximumRetries != 0)
            {
                MAXIMUM_RETRIES_ALLOWED = maximumRetries;
            }
            else
            {
                MAXIMUM_RETRIES_ALLOWED = 6;
            }

            // allowed difference depends on retry interval
            if (MINUTES_RETRY_INTERVAL < 2)
            {
                MINUTES_ALLOWED_DIFFERENCE = 2;
            }
            else
            {
                MINUTES_ALLOWED_DIFFERENCE = MINUTES_RETRY_INTERVAL;
            }
        }

        public List<long> GetAllPartnerIds()
        {
            return _repository.GetAllPartnerIds();
        }

        public bool UpdateOrInsertHouseholdRecording(int groupId, long userId, long householdId, Recording recording,
            string recordingKey, TstvRecordingStatus status, bool scheduledSaved)
        {
            DateTime date = DateTime.UtcNow;
            HouseholdRecording householdRecording = new HouseholdRecording(userId, householdId, recording.EpgId,
                recordingKey, status.ToString(),
                recording.Type.ToString(), date, date, recording.ProtectedUntilDate, recording.ChannelId,
                scheduledSaved);

            bool result = true;
            var current =
                _repository.GetHouseholdRecording(groupId, recordingKey, householdId);
            
            // if (_repository.IsHouseholdRecordingExists(groupId, recordingKey, householdId))//Has old hh recording is status canceled
            if (current != null && current.Id > 0)//Has old hh recording is status canceled
            {
                householdRecording.Id = current.Id;
                result = _repository.UpdateHouseholdRecording(groupId, householdRecording);
            }
            else
            {
                householdRecording.Id = _repository.AddHouseholdRecording(groupId, householdRecording);
            }

            // recording.Id = householdRecording.Id;
            return result;
        }

        public Recording GetHouseholdRecording(int groupId, long householdId, long epgId, string recordingKey)
        {
            HouseholdRecording householdRecording =
                _repository.GetHouseholdRecording(groupId, recordingKey, householdId);
            if (householdRecording != null && householdRecording.Id > 0)
            {
                Program program = _repository.GetProgramByEpg(groupId, epgId);
                TimeBasedRecording timeBasedRecording = _repository.GetRecordingByKey(groupId, recordingKey);

                if (timeBasedRecording != null && program != null && householdRecording != null)
                {
                    return RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, program,
                        householdRecording);
                }
            }

            return null;
        }

        public Recording GetHouseholdRecordingById(int groupId, long id, long householdId)
        {
            HouseholdRecording householdRecording =
                _repository.GetHouseholdRecordingById(groupId, id, householdId, DomainRecordingStatus.OK.ToString());
            if (householdRecording != null)
            {
                Program program = _repository.GetProgramByEpg(groupId, householdRecording.EpgId);
                TimeBasedRecording timeBasedRecording =
                    _repository.GetRecordingByKey(groupId, householdRecording.RecordingKey);

                if (timeBasedRecording != null && program != null && householdRecording != null)
                {
                    return RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, program,
                        householdRecording);
                }
            }

            return null;
        }

        public Dictionary<long, Recording> GetHouseholdRecordingsByRecordingStatuses(int groupId, long householdId,
            List<DomainRecordingStatus> recordingStatuses)
        {
            List<string> recordingStatusesStrings = recordingStatuses.ConvertAll(f => f.ToString());
            List<HouseholdRecording> householdRecordings =
                _repository.GetHhRecordingsByHhIdAndRecordingStatuses(groupId, householdId, recordingStatusesStrings);

            return GetRecordingByHouseholdRecording(groupId, householdRecordings);
        }

        public static string GetRecordingKey(long epgId, int paddingBefore, int paddingAfter)
        {
            return string.Format($"{epgId}_{paddingBefore}_{paddingAfter}");
        }

        public Dictionary<long, Recording> GetHouseholdProtectedRecordings(int groupId, long householdId)
        {
            List<HouseholdRecording> householdRecordings =
                _repository.GetHouseholdProtectedRecordings(groupId, householdId,
                    TVinciShared.DateUtils.GetUtcUnixTimestampNow());

            return GetRecordingByHouseholdRecording(groupId, householdRecordings);
        }

        private Dictionary<long, Recording> GetRecordingByHouseholdRecording(int groupId,
            List<HouseholdRecording> householdRecordings)
        {
            Dictionary<long, Recording> domainRecordingIdToRecordingMap = new Dictionary<long, Recording>();
            List<long> epgIdIds = householdRecordings.Select(e => e.EpgId).ToList();
            List<string> paddedRecordingKeys = householdRecordings.Select(e => e.RecordingKey).ToList();
            List<Program> programs = _repository.GetProgramsByEpgIds(groupId, epgIdIds);
            List<TimeBasedRecording> paddedRecordings = _repository.GetRecordingsByKeys(groupId, paddedRecordingKeys);

            foreach (var householdRecording in householdRecordings)
            {
                Program program = programs.Find(f => f.EpgId.Equals(householdRecording.EpgId));
                TimeBasedRecording timeBasedRecording =
                    paddedRecordings.Find(f => f.Key.Equals(householdRecording.RecordingKey));
                if (program != null && timeBasedRecording != null)
                {
                    domainRecordingIdToRecordingMap.Add(householdRecording.Id,
                        RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, program, householdRecording));
                }
            }

            return domainRecordingIdToRecordingMap;
        }

        public Dictionary<long, Recording> GetHouseholdRecordingsToRecover(int groupId, long householdId)
        {
            Dictionary<long, Recording> domainRecordingIdToRecordingMap =
                GetHouseholdRecordingsByRecordingStatuses(groupId, householdId,
                    new List<DomainRecordingStatus>() { DomainRecordingStatus.DeletePending });
            if (domainRecordingIdToRecordingMap.Count > 0)
            {
                var acountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
                int recoveryGracePeriod = RECOVERY_GRACE_PERIOD; // default value 10 days 
                if (acountSettings != null && acountSettings.RecoveryGracePeriod.HasValue)
                {
                    recoveryGracePeriod = acountSettings.RecoveryGracePeriod.Value;
                }

                domainRecordingIdToRecordingMap = domainRecordingIdToRecordingMap.Where(x =>
                        x.Value.UpdateDate.AddSeconds(recoveryGracePeriod) >= DateTime.UtcNow)
                    .OrderByDescending(kvp => kvp.Value.EpgStartDate)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            return domainRecordingIdToRecordingMap;
        }

        public bool UpdateHouseholdRecordingsStatus(int partnerId, List<long> domainRecordingIds, string recordingState)
        {
            var hhRecordings =
                _repository.UpdateHouseholdRecordingsStatus(partnerId, domainRecordingIds, recordingState);
            if (hhRecordings.Count > 0)
            {
                foreach (var householdRecording in hhRecordings)
                {
                    LayeredCache.Instance.SetInvalidationKey(
                        LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId,
                            householdRecording.HouseholdId));
                }

                return true;
            }

            return false;
        }

        public Status ValidateAddRecording(int partnerId, DateTime startDate, int? paddingBefore)
        {
            Status status = new Status(eResponseStatus.OK);

            if (startDate.AddMinutes((-1 * paddingBefore) ?? 0) < DateTime.UtcNow)
            {
                status = new Status(eResponseStatus.CanOnlyAddRecordingBeforeRecordingStart,
                    "can only add recording before recording start");
            }

            return status;
        }

        public Recording Record(int groupId, long programId, long epgChannelID, DateTime startDate, DateTime endDate,
            string crid,
            List<long> domainIds, out HashSet<long> failedDomainIds, int? paddingBefore, int? paddingAfter,
            RecordingContext recordingContext = RecordingContext.Regular)
        {
            Recording recording = null;
            failedDomainIds = null;
            TimeBasedRecording timeBasedRecording = null;
            string syncKey = string.Format("PaddedRecordingsManager_{0}", programId);
            Dictionary<string, object> syncParmeters = new Dictionary<string, object>();
            syncParmeters.Add("groupId", groupId);
            syncParmeters.Add("programId", programId);
            syncParmeters.Add("crid", crid);
            syncParmeters.Add("epgChannelID", epgChannelID);
            syncParmeters.Add("startDate", startDate);
            syncParmeters.Add("endDate", endDate);
            syncParmeters.Add("domainIds", new List<long>() { });
            syncParmeters.Add("isPrivateCopy", false);
            syncParmeters.Add("recordingContext", recordingContext);
            syncParmeters.Add("paddingBefore", paddingBefore);
            syncParmeters.Add("paddingAfter", paddingAfter);
            syncParmeters.Add("absoluteStart", null);
            syncParmeters.Add("absoluteEnd", null);
            try
            {
                var key = GetRecordingKey(programId, paddingBefore ?? 0, paddingAfter ?? 0);
                timeBasedRecording = _repository.GetRecordingByKey(groupId, key);
                var shouldIssueRecord = ShouldIssueRecord(timeBasedRecording);

                syncParmeters.Add("shouldInsertRecording", shouldIssueRecord);

                if (shouldIssueRecord)
                {
                    long recordingId = 0;
                    bool syncedAction = synchronizer.DoAction(syncKey, syncParmeters);

                    object recordingObject;
                    if (syncParmeters.TryGetValue("recording", out recordingObject))
                    {
                        recording = (Recording)recordingObject;
                        recordingId = recording.Id;
                    }
                    // all good
                    else
                    {
                        timeBasedRecording = _repository.GetRecordingByKey(groupId, key);
                        recordingId = timeBasedRecording.Id;
                    }

                    // Schedule a message to check duplicate crid
                    DateTime checkTime = endDate.AddDays(7);
                    eRecordingTask task = eRecordingTask.CheckRecordingDuplicateCrids;
                    RecordingsManager.EnqueueMessage(groupId, programId, recordingId, startDate, checkTime,
                        task);
                }

                Program program = _repository.GetProgramByEpg(groupId, programId);
                if (recording == null && program != null && timeBasedRecording != null)
                {
                    recording = RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, program);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("PaddedRecordingsManager - Record: in record of program {0}. error = {1}", programId,
                    ex);
                recording = null;
            }

            return recording;
        }

        private bool ShouldIssueRecord(TimeBasedRecording timeBasedRecording)
        {
            if (timeBasedRecording == null || timeBasedRecording.Id == 0)
                return true;

            var reIssueStatuses = new List<RecordingInternalStatus>
                { RecordingInternalStatus.Canceled, RecordingInternalStatus.Deleted }.Select(s=>s.ToString());
            return reIssueStatuses.Contains(timeBasedRecording.Status);
        }

        public Status DeleteRecording(int groupId, Recording slimRecording)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (slimRecording?.Id > 0 && slimRecording?.EpgId > 0)
            {
                bool isLastPaddedRecording;
                string recordingKey = "";

                if (slimRecording.AbsoluteStartTime.HasValue)
                {
                    recordingKey = GetImmediateRecordingKey(slimRecording.EpgId,
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(slimRecording.AbsoluteStartTime),
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(slimRecording.AbsoluteEndTime));
                }
                else if (slimRecording.StartPadding.HasValue && slimRecording.EndPadding.HasValue)
                {
                    recordingKey = GetRecordingKey(slimRecording.EpgId, slimRecording.StartPadding.Value,
                        slimRecording.EndPadding.Value);
                }

                isLastPaddedRecording =
                    _repository.GetTop2HouseholdRecordingsByKey(groupId, recordingKey,
                        RecordingInternalStatus.OK.ToString()).Count < 2;

                // if last recording then update ES and CB
                if (isLastPaddedRecording)
                {
                    int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
                    string externalChannelId = CatalogDAL.GetEPGChannelCDVRId(groupId, slimRecording.ChannelId);
                    TimeShiftedTvPartnerSettings accountSettings =
                        Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);

                    var recToDeleteDate = GetActualEndDate(accountSettings, slimRecording);
                    bool delete = recToDeleteDate < DateTime.UtcNow;

                    status = DeleteRecordingAdapter(groupId, slimRecording.ExternalRecordingId, slimRecording.EpgId,
                        externalChannelId, adapterId, delete);

                    if (!status.IsOkStatusCode())
                        return status;

                    List<TimeBasedRecording> paddedRecordings =
                        _repository.GetRecordingsByEpgIdAndStatus(groupId, slimRecording.EpgId,
                            RecordingInternalStatus.OK.ToString());

                    if (paddedRecordings.Count == 1)
                    {
                        if (delete)
                        {
                            _repository.DeleteProgram(groupId, paddedRecordings[0].ProgramId);
                        }

                        RecordingsUtils.UpdateCouchbase(groupId, slimRecording.EpgId, paddedRecordings[0].ProgramId,
                            true);
                        RecordingsUtils.UpdateIndex(groupId, paddedRecordings[0].ProgramId, eAction.Delete);
                    }

                    InternalModifyRecording(groupId, slimRecording.Id, recordingKey, recToDeleteDate, true);

                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return status;
        }

        public GenericResponse<Recording> DeleteHouseholdRecordings(int partnerId, long domainRecordingId,
            string userId)
        {
            var response = new GenericResponse<Recording>();
            response.Status.Set(new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()));
            long householdId = 0;
            var status = Core.ConditionalAccess.Utils.ValidateUserAndDomain(partnerId, userId, ref householdId, out _);

            if (!status.IsOkStatusCode())
            {
                log.Debug(
                    $"User or Domain not valid. {nameof(partnerId)}:{partnerId}, {nameof(userId)}:{userId}, {nameof(householdId)}:{householdId}.");
                response.Status.Set(status);
                return response;
            }

            var recording = GetHouseholdRecordingById(partnerId, domainRecordingId, householdId);
            status = ValidateDeleteRecording(partnerId, recording);
            if (!status.IsOkStatusCode())
            {
                log.Debug(
                    $"Validate delete recording failed. {nameof(partnerId)}:{partnerId}, {nameof(userId)}:{userId}, {nameof(householdId)}:{householdId}.");
                response.Status.Set(status);
                return response;
            }

            if (_repository.DeleteHouseholdRecordingById(partnerId, domainRecordingId))
            {
                LayeredCache.Instance.SetInvalidationKey(
                    LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId,
                        householdId));
                DeleteRecording(partnerId, recording);
            }

            response.Object = recording;
            return response;
        }

        public Status ValidateDeleteRecording(int partnerId, Recording recording)
        {
            Status status = new Status(eResponseStatus.OK);
            if (recording == null)
            {
                status = new Status(eResponseStatus.RecordingNotFound,
                    "Recording not found");
            }
            else if ((recording.AbsoluteEndTime.HasValue && recording.AbsoluteEndTime.Value > DateTime.UtcNow) ||
                     (recording.EndPadding.HasValue &&
                      recording.EpgEndDate.AddMinutes(recording.EndPadding.Value) > DateTime.UtcNow))
            {
                status = new Status(eResponseStatus.CanOnlyDeleteRecordingAfterRecordingEnd,
                    "Can only delete recording after recording end");
            }

            return status;
        }

        public GenericResponse<Recording> CancelHouseholdRecordings(int partnerId, long hhRecordingId, string userId)
        {
            var response = new GenericResponse<Recording>();
            Recording recording = new Recording();
            response.Status.Set(new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()));
            long householdId = 0;
            var status = Core.ConditionalAccess.Utils.ValidateUserAndDomain(partnerId, userId, ref householdId, out _);
            if (!status.IsOkStatusCode())
            {
                log.Debug(
                    $"User or Domain not valid. {nameof(partnerId)}:{partnerId}, {nameof(userId)}:{userId}, {nameof(householdId)}:{householdId}.");
                response.Status.Set(status);
                return response;
            }

            recording = GetHouseholdRecordingById(partnerId, hhRecordingId, householdId);

            status = ValidateCancelRecording(partnerId, recording);
            if (!status.IsOkStatusCode())
            {
                log.Debug(
                    $"Validate cancel recording failed. {nameof(partnerId)}:{partnerId}, {nameof(userId)}:{userId}, {nameof(householdId)}:{householdId}.");
                response.Status.Set(status);
                return response;
            }

            UpdateHouseholdRecordingsStatus(partnerId, new List<long>() { hhRecordingId },
                DomainRecordingStatus.Canceled.ToString());
            DeleteRecording(partnerId, recording);
            recording.RecordingStatus = TstvRecordingStatus.Canceled;
            response.Object = recording;
            return response;
        }

        public Status ValidateCancelRecording(int partnerId, Recording recording)
        {
            Status status = new Status(eResponseStatus.OK);
            if (recording == null)
            {
                status = new Status(eResponseStatus.RecordingNotFound,
                    "Recording not found");
            }
            else if ((recording.AbsoluteEndTime.HasValue && recording.AbsoluteEndTime.Value < DateTime.UtcNow) ||
                     (recording.EndPadding.HasValue &&
                      recording.EpgEndDate.AddMinutes(recording.EndPadding.Value) < DateTime.UtcNow))
            {
                status = new Status(eResponseStatus.CanOnlyCancelRecordingBeforeRecordingEnd,
                    "Can only cancel recording before recording end");
            }

            return status;
        }

        private Status InternalModifyRecording(int partnerId, long recordingId, string recordingKey,
            DateTime epgEndDate,
            bool sendModificationEvent)
        {
            log.Debug($"InternalModifyRecording recording:{recordingId}");

            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // Update all domains that have this recording
            if (sendModificationEvent)
            {
                try
                {
                    SendEvictRecording(partnerId, recordingId, 0,
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow));
                }
                catch (Exception e)
                {
                    log.Error(
                        $"Failed to queue ExpiredRecording task for CancelOrDeleteRecording, recordingId: {recordingId}, groupId: {partnerId}",
                        e);
                }
            }

            try
            {
                bool isOk = true;
                if (epgEndDate > DateTime.UtcNow)
                {
                    isOk = _repository.UpdateRecordingStatus(partnerId, recordingId,
                        RecordingInternalStatus.Canceled.ToString());
                }
                else
                {
                    isOk = _repository.DeleteRecording(partnerId, recordingKey);
                }

                if (isOk)
                {
                    status = new Status((int)eResponseStatus.OK);
                }
                else
                {
                    return new Status((int)eResponseStatus.Error,
                        "Failed CancelRecording or DeleteRecording for on DB");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error on internalExpireRecording, recordingID: {0}", recordingId), ex);
                status = new Status((int)eResponseStatus.Error,
                    "Exception on CancelRecording or DeleteRecording on DB");
            }

            return status;
        }

        /// <summary>
        /// Actually performs the "Record"
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private bool synchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool success = true;

            int groupId = 0;
            if (parameters.TryGetValue("groupId", out var _groupId))
                groupId = (int)_groupId;

            long epgId = 0;
            if (parameters.TryGetValue("programId", out var _epgId))
                epgId = (long)_epgId;

            string crid = string.Empty;
            if (parameters.TryGetValue("crid", out var _crid))
                crid = _crid.ToString();

            long epgChannelID = 0;
            if (parameters.TryGetValue("epgChannelID", out var _epgChannelId))
                epgChannelID = (long)_epgChannelId;

            DateTime startDate = default(DateTime);
            if (parameters.TryGetValue("startDate", out var _startDate))
                startDate = (DateTime)_startDate;

            DateTime endDate = default(DateTime);
            if (parameters.TryGetValue("endDate", out var _endDate))
                endDate = (DateTime)_endDate;

            var domainIds = new List<long>();
            if (parameters.TryGetValue("domainIds", out var _domainIds))
                domainIds = (List<long>)_domainIds;

            bool isPrivateCopy = false;
            if (parameters.TryGetValue("isPrivateCopy", out var _isPrivateCopy))
                isPrivateCopy = (bool)_isPrivateCopy;

            bool shouldInsertRecording = false;
            if (parameters.TryGetValue("shouldInsertRecording", out var _shouldInsertRecording))
                shouldInsertRecording = (bool)_shouldInsertRecording;

            RecordingContext recordingContext = parameters.ContainsKey("recordingContext")
                ? (RecordingContext)parameters["recordingContext"]
                : RecordingContext.Regular;

            int paddingBefore = 0;
            if (parameters.TryGetValue("paddingBefore", out var _paddingBefore))
                paddingBefore = (int)_paddingBefore;

            int paddingAfter = 0;
            if (parameters.TryGetValue("paddingAfter", out var _paddingAfter))
                paddingAfter = (int)_paddingAfter;

            DateTime? absoluteStart = null;
            if (parameters.TryGetValue("absoluteStart", out var _absoluteStart))
                absoluteStart = (DateTime?)_absoluteStart;

            DateTime? absoluteEnd = null;
            if (parameters.TryGetValue("absoluteEnd", out var _absoluteEnd))
                absoluteEnd = (DateTime?)_absoluteEnd;

            TimeShiftedTvPartnerSettings accountSettings =
                ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);

            int? recordingLifetime = accountSettings.RecordingLifetimePeriod;
            DateTime? viewableUntilDate = null;
            if (recordingLifetime.HasValue)
                viewableUntilDate = endDate.AddDays(recordingLifetime.Value);

            var isStopped = false;
            if (parameters.TryGetValue("isStopped", out var _isStopped))
                isStopped = (bool)_isStopped;
            
            // for private copy we always issue a recording            
            //Recording recording = ConditionalAccess.Utils.GetRecordingByEpgId(groupId, epgId);
            Program program = _repository.GetProgramByEpg(groupId, epgId);

            var isImmediate = absoluteStart.HasValue && absoluteStart.Value > default(DateTime) ||
                              absoluteEnd.HasValue && absoluteEnd.Value > default(DateTime);
            string key;
            if (isImmediate)
            {
                var epocStart = DateUtils.DateTimeToUtcUnixTimestampSeconds(absoluteStart.Value);
                var epocEnd = DateUtils.DateTimeToUtcUnixTimestampSeconds(absoluteEnd.Value);
                key = GetImmediateRecordingKey(epgId, epocStart, epocEnd);
            }
            else
            {
                key = GetRecordingKey(epgId, paddingBefore, paddingAfter);
            }

            TimeBasedRecording recording;
            if (program == null)
            {
                program = new Program(epgId, startDate, endDate);
                program.Id = _repository.AddProgram(groupId, new Program(epgId, startDate, endDate));
                recording = new TimeBasedRecording(key, epgId, epgChannelID, program.Id,
                    RecordingInternalStatus.Waiting.ToString(),
                    DateTime.UtcNow, DateTime.UtcNow, viewableUntilDate.Value,
                    TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(viewableUntilDate.Value),
                    false, crid, paddingBefore, paddingAfter);

                if (isImmediate)
                {
                    recording.AbsoluteStartTime = absoluteStart;
                    recording.AbsoluteEndTime = absoluteEnd;
                }

                recording.Id = _repository.AddRecording(groupId, recording);

                RecordingsUtils.UpdateCouchbase(groupId, epgId, program.Id);
                RecordingsUtils.UpdateIndex(groupId, program.Id, eAction.Update);
            }
            else
            {
                recording = _repository.GetRecordingByKey(groupId, key);
                if (recording == null || recording.Id == 0)
                {
                    recording = new TimeBasedRecording(key, epgId, epgChannelID, program.Id,
                        RecordingInternalStatus.Waiting.ToString(),
                        DateTime.UtcNow, DateTime.UtcNow, viewableUntilDate.Value,
                        TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(viewableUntilDate.Value),
                        false, crid, paddingBefore, paddingAfter);

                    if (isImmediate)
                    {
                        recording.AbsoluteStartTime = absoluteStart;
                        recording.AbsoluteEndTime = absoluteEnd;
                    }

                    recording.Id = _repository.AddRecording(groupId, recording);
                }
                else if (recording.Status == RecordingInternalStatus.Canceled.ToString())
                {
                    var id = recording.Id;
                    
                    recording = new TimeBasedRecording(key, epgId, epgChannelID, program.Id,
                        RecordingInternalStatus.Waiting.ToString(),
                        DateTime.UtcNow, DateTime.UtcNow, viewableUntilDate.Value,
                        TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(viewableUntilDate.Value),
                        false, crid, paddingBefore, paddingAfter);

                    if (isImmediate)
                    {
                        recording.AbsoluteStartTime = absoluteStart;
                        recording.AbsoluteEndTime = absoluteEnd;
                    }

                    recording.Id = id;
                    
                    if(!_repository.UpdateRecording(groupId, recording))
                        log.Error($"Failed updating recording: {recording.Id} in status Canceled");
                }
            }

            if (isImmediate)
            {
                startDate = absoluteStart.Value;
                endDate = absoluteEnd.Value;
            }
            else
            {
                startDate = startDate.AddMinutes(-1 * paddingBefore);
                endDate = endDate.AddMinutes(paddingAfter);
            }

            if (shouldInsertRecording)
            {
                var checkTime = DateTime.UtcNow > endDate
                    ? DateTime.UtcNow.AddMinutes(1)
                    : endDate.AddMinutes(paddingAfter + 1);
                SendVerifyRecordingFinalStatus(groupId, recording.Id, checkTime);
            }

            // Async - call adapter. Main flow is done
            LogContextData contextData = new LogContextData();
            Task async = Task.Run(() =>
            {
                contextData.Load();
                CallAdapterRecord(groupId, epgId.ToString(), epgChannelID, startDate, endDate, recording,
                    domainIds, out HashSet<long> failedDomainIds, recordingContext);

                if (failedDomainIds?.Count > 0)
                {
                    parameters["failedDomainIds"] = failedDomainIds;
                }
            });

            if (program != null && recording != null)
            {
                parameters["recording"] = RecordingsUtils.BuildRecordingFromTBRecording(groupId, recording, program, null, isStopped);
            }

            return success;
        }

        private void CallAdapterRecord(int groupId, string epgId, long epgChannelID, DateTime startDate,
            DateTime endDate, TimeBasedRecording currentRecording,
            List<long> domainIds, out HashSet<long> failedDomainIds,
            RecordingContext context = RecordingContext.Regular)
        {
            log.DebugFormat("Call adapter record for recording {0}", currentRecording.Id);
            bool shouldRetry = true;
            Status status = null;
            int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
            var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();
            failedDomainIds = null;
            // Initialize parameters for adapter controller
            long startTimeSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(startDate);
            long durationSeconds = (long)(endDate - startDate).TotalSeconds;
            string externalChannelId = CatalogDAL.GetEPGChannelCDVRId(groupId, epgChannelID);

            RecordResult adapterResponse = null;
            try
            {
                adapterResponse = adapterController.Record(groupId, startTimeSeconds, durationSeconds, epgId,
                    externalChannelId, adapterId, domainIds);
            }
            catch (KalturaException ex)
            {
                status = new Status((int)eResponseStatus.Error,
                    string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
            }
            catch (Exception ex)
            {
                status = new Status((int)eResponseStatus.Error, "Adapter controller exception: " + ex.Message);
            }

            if (adapterResponse == null)
            {
                status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
            }

            try
            {
                RecordingInternalStatus newRecordingInternalStatus = RecordingInternalStatus.Waiting;

                if (adapterResponse != null)
                {
                    // Set external recording ID
                    currentRecording.ExternalId = adapterResponse.RecordingId;
                }
                else
                {
                    shouldRetry = true;
                }

                // if adapter failed - retry, don't mark as failed
                if (status != null && status.Code != (int)eResponseStatus.OK)
                {
                    shouldRetry = true;
                }

                // If we have a response AND we didn't set the status to be invalid
                if (adapterResponse != null && (status == null || status.Code == (int)eResponseStatus.OK))
                {
                    // if provider failed
                    if (!adapterResponse.ActionSuccess || adapterResponse.FailReason != 0)
                    {
                        shouldRetry = true;
                    }
                    else
                    {
                        currentRecording.Status = RecordingsUtils.ConvertToRecordingInternalStatus(RecordingsUtils.GetTstvRecordingStatus(startDate, endDate, TstvRecordingStatus.Scheduled))
                            .ToString();
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        // Insert the new links of the recordings
                        if (adapterResponse.Links != null && adapterResponse.Links.Count > 0)
                        {
                            RecordingsDAL.InsertRecordingLinks(adapterResponse.Links, groupId, currentRecording.Id);
                        }

                        if (context != RecordingContext.PrivateDistribute)
                        {
                            shouldRetry = adapterResponse.FailedDomainIds?.Count > 0;

                            if (context == RecordingContext.PrivateRetry)
                            {
                                var newSuccess = domainIds.Except(adapterResponse.FailedDomainIds).ToList();

                                if (newSuccess?.Count > 0)
                                {
                                    //Update successful retry
                                    _repository.UpdateHouseholdRecordingsFailure(groupId, newSuccess, true);
                                }
                            }

                            if (context == RecordingContext.Regular && shouldRetry)
                            {
                                _repository.UpdateHouseholdRecordingsFailure(groupId, adapterResponse.FailedDomainIds,
                                    false);
                            }
                        }

                        newRecordingInternalStatus = RecordingInternalStatus.OK;
                    }
                }

                currentRecording.Status = newRecordingInternalStatus.ToString();
                // Update the result from the adapter
                bool updateSuccess = _repository.UpdateRecording(groupId, currentRecording);

                if (!updateSuccess)
                {
                    status = new Status((int)eResponseStatus.Error, "Failed updating recording in database.");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed inserting/updating recording {0} in database and queue: {1}",
                    currentRecording.Id, ex);
                status = new Status((int)eResponseStatus.Error,
                    "Failed inserting/updating recording in database and queue.");
            }

            if (context != RecordingContext.PrivateDistribute && shouldRetry)
            {
                log.DebugFormat("Call adapter record for recording {0} will retry", currentRecording.Id);
                RetryTaskBeforeProgramStarted(groupId, startDate, currentRecording);
            }
        }

        private bool IsRecordingBeingUsedByMultipleHouseholds(int groupId, string recordingKey,
            List<DomainRecordingStatus> recordingStatuses)
        {
            List<string> recordingStatusesStrings = recordingStatuses.ConvertAll(f => f.ToString());
            List<HouseholdRecording> householdRecordings =
                _repository.ListHouseholdRecordingsByRecordingKey(groupId, recordingKey, recordingStatusesStrings);
            return householdRecordings != null && householdRecordings.Count > 1;
        }

        public GenericResponse<Recording> StopRecord(ContextData contextData, long epgId, long epgChannelId,
            long householdRecordingId)
        {
            var recording = new GenericResponse<Recording>();
            TimeBasedRecording timeBasedRecording = null;
            recording.SetStatus(eResponseStatus.OK);
            string syncKey = string.Format("PaddedRecordingsManager_{0}", epgId);
            Dictionary<string, object> syncParmeters = new Dictionary<string, object>();
            syncParmeters.Add("groupId", contextData.GroupId);
            syncParmeters.Add("programId", epgId);
            syncParmeters.Add("epgChannelID", epgChannelId);
            syncParmeters.Add("domainIds", new List<long>() { });
            syncParmeters.Add("isPrivateCopy", false);

            try
            {
                //1. Check if program or padding has started
                //2. Check if program is being recorded by HH
                var hhRecording = _repository.GetHouseholdRecordingById(contextData.GroupId, householdRecordingId,
                    contextData.DomainId ?? 0, DomainRecordingStatus.OK.ToString());

                if (hhRecording == null)
                {
                    recording.SetStatus((int)eResponseStatus.NotAllowed,
                        "Program is not being recorded");
                    return recording;
                }

                if (!hhRecording.EpgId.Equals(epgId) ||
                    hhRecording.EpgChannelId > 0 && !hhRecording.EpgChannelId.Equals(epgChannelId))
                {
                    recording.SetStatus((int)eResponseStatus.NotAllowed,
                        "Inconsistent program or channel identifier");
                    return recording;
                }

                var oldRecording = _repository.GetRecordingByKey(contextData.GroupId, hhRecording.RecordingKey);

                if (oldRecording == null)
                {
                    recording.SetStatus((int)eResponseStatus.RecordingNotFound,
                        $"Recording {hhRecording.Id} wasn't found");
                    return recording;
                }

                var programByEpg = _repository.GetProgramByEpg(contextData.GroupId, oldRecording.EpgId);
                if (programByEpg.StartDate.AddMinutes(-1 * oldRecording.PaddingBeforeMins) > DateTime.UtcNow)
                {
                    recording.SetStatus(eResponseStatus.RecordingStatusNotValid,
                        "Stop is not allowed for programs in the future");
                    return recording;
                }

                if (programByEpg.EndDate.AddMinutes(oldRecording.PaddingAfterMins) < DateTime.UtcNow)
                {
                    recording.SetStatus(eResponseStatus.RecordingStatusNotValid,
                        "Stop is not allowed for programs that already ended");
                    return recording;
                }

                var isScheduledRecording = IsScheduledRecording(oldRecording, programByEpg);
                if (!isScheduledRecording.IsOkStatusCode())
                {
                    recording.SetStatus(isScheduledRecording);
                    return recording;
                }

                var accountSettings = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(contextData.GroupId);
                //3. Create new permutation with the original padding and change the end padding
                var absoluteStartTime = GetActualStartDate(accountSettings,
                    RecordingsUtils.BuildRecordingFromTBRecording(contextData.GroupId, oldRecording, programByEpg));

                var absoluteStartTimeEpoch =
                    DateUtils.DateTimeToUtcUnixTimestampSeconds(absoluteStartTime);
                var absoluteEndTime = DateTime.UtcNow.RoundUp(TimeSpan.FromMinutes(1));
                var absoluteEndTimeEpoch =
                    DateUtils.DateTimeToUtcUnixTimestampSeconds(absoluteEndTime);

                if (absoluteEndTime < absoluteStartTime) //due to round up & down
                {
                    //0 seconds recording
                    absoluteEndTime = absoluteStartTime;
                    absoluteEndTimeEpoch = absoluteStartTimeEpoch;
                }

                syncParmeters.Add("absoluteStart", absoluteStartTime);
                syncParmeters.Add("absoluteEnd", absoluteEndTime);
                syncParmeters.Add("endDate", programByEpg.EndDate);
                syncParmeters.Add("isStopped", true);

                var newRecordingKey =
                    GetImmediateRecordingKey(hhRecording.EpgId, absoluteStartTimeEpoch, absoluteEndTimeEpoch);

                //4. Check if anyone else is using the original permutation
                var statuses = new List<DomainRecordingStatus> { DomainRecordingStatus.OK, DomainRecordingStatus.None };
                var isOldRecordingBeingUsedByMultipleHouseholds =
                    IsRecordingBeingUsedByMultipleHouseholds(contextData.GroupId, hhRecording.RecordingKey, statuses);

                if (!isOldRecordingBeingUsedByMultipleHouseholds)
                {
                    //Already has new permutation
                    if (_repository.GetRecordingByKey(contextData.GroupId, newRecordingKey) != null)
                    {
                        //delete old
                        var recordingToDelete = new Recording()
                        {
                            Id = oldRecording.Id,
                            EpgId = oldRecording.EpgId,
                            StartPadding = oldRecording.PaddingBeforeMins,
                            EndPadding = oldRecording.PaddingAfterMins,
                            ChannelId = oldRecording.EpgChannelId,
                            EpgEndDate = programByEpg.EndDate,
                            AbsoluteStartTime = oldRecording.AbsoluteStartTime,
                            AbsoluteEndTime = oldRecording.AbsoluteEndTime
                        };
                        var deleteRecording =
                            DeleteRecording(contextData.GroupId, recordingToDelete);
                        if (!deleteRecording.IsOkStatusCode())
                        {
                            log.Error(
                                $"Couldn't delete recording: {recordingToDelete.Id}, code: {deleteRecording.Code}, message: {deleteRecording.Message}");
                        }
                    }
                    else
                    {
                        //New permutation, update old record
                        oldRecording.Key = newRecordingKey;
                        oldRecording.AbsoluteStartTime = absoluteStartTime;
                        oldRecording.AbsoluteEndTime = absoluteEndTime;

                        if (!UpdateRecording(contextData.GroupId, programByEpg, oldRecording))
                        {
                            var msg = $"Couldn't update recording: {oldRecording.Id}";
                            log.Error(msg);
                            recording.SetStatus(eResponseStatus.Error, msg);
                            return recording;
                        }

                        var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();
                        var adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(contextData.GroupId);
                        var externalChannelId = CatalogDAL.GetEPGChannelCDVRId(contextData.GroupId, epgChannelId);
                        var durationSeconds = GetImmediateRecordingTimeSpanSeconds(absoluteStartTime, absoluteEndTime);
                        var adapterResponse = adapterController.UpdateRecordingSchedule(contextData.GroupId,
                            epgId.ToString(), externalChannelId,
                            oldRecording.ExternalId, adapterId, absoluteStartTimeEpoch, durationSeconds);
                        if (!adapterResponse.ActionSuccess)
                        {
                            log.Error($"Failed to update recording: {oldRecording.Id} with new key: {newRecordingKey}");
                            recording.SetStatus(eResponseStatus.Error, "Failed to modify recording");
                            return recording;
                        }
                    }
                }

                //5. Update HH ref to the new permutation
                var newHhRecording = UpdateHouseholdRecording(contextData.GroupId, householdRecordingId,
                    contextData.DomainId ?? 0, hhRecording.RecordingKey, newRecordingKey,
                    hhRecording.Status, DomainRecordingStatus.OK.ToString());

                if (newHhRecording == null)
                {
                    log.Error(
                        $"Couldn't update recording with key: {newRecordingKey} for hh: {contextData.DomainId ?? 0}");
                    recording.SetStatus(eResponseStatus.Error);
                    return recording;
                }

                LayeredCache.Instance.SetInvalidationKey(
                    LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(contextData.GroupId,
                        contextData.DomainId ?? 0));

                var existingPermutation = _repository.GetRecordingByKey(contextData.GroupId, newRecordingKey);
                var shouldIssueRecord = ShouldIssueRecord(existingPermutation);
                
                syncParmeters.Add("shouldInsertRecording", shouldIssueRecord);

                if (shouldIssueRecord)
                {
                    long recordingId = 0;
                    synchronizer.DoAction(syncKey, syncParmeters);

                    if (syncParmeters.TryGetValue("recording", out var recordingObject))
                    {
                        recording.Object = (Recording)recordingObject;
                        recordingId = recording.Object.Id;
                    }
                    // all good
                    else
                    {
                        timeBasedRecording = _repository.GetRecordingByKey(contextData.GroupId, newRecordingKey);
                        recordingId = timeBasedRecording.Id;
                    }

                    // Schedule a message to check duplicate crid
                    DateTime checkTime = programByEpg.EndDate.AddDays(7);
                    eRecordingTask task = eRecordingTask.CheckRecordingDuplicateCrids;
                    RecordingsManager.EnqueueMessage(contextData.GroupId, epgId, recordingId,
                        programByEpg.StartDate,
                        checkTime,
                        task);
                }

                var program = _repository.GetProgramByEpg(contextData.GroupId, epgId);
                if (recording.Object == null && program != null)
                {
                    if (timeBasedRecording == null)
                        timeBasedRecording = _repository.GetRecordingByKey(contextData.GroupId, newRecordingKey);

                    recording.Object = RecordingsUtils.BuildRecordingFromTBRecording(contextData.GroupId, timeBasedRecording, program,
                        newHhRecording, true);
                }

                if (recording.Object != null)
                {
                    recording.Object.AbsoluteStartTime = absoluteStartTime;
                    recording.Object.AbsoluteEndTime = absoluteEndTime;
                    recording.Object.StartPadding = 0; //set abs instead
                    recording.Object.EndPadding = 0; //set abs instead
                    recording.Object.Id = householdRecordingId;
                }
            }
            catch (Exception ex)
            {
                log.Error(
                    $"PaddedRecordingsManager - Stop hhRecordingId: {householdRecordingId}: in record of program {epgId}",
                    ex);
                recording.SetStatus(eResponseStatus.Error);
            }

            return recording;
        }

        private Status IsScheduledRecording(TimeBasedRecording oldRecording, Program programByEpg)
        {
            var recordingInternalStatus =
                (RecordingInternalStatus)Enum.Parse(typeof(RecordingInternalStatus), oldRecording.Status);
            var recordingStatus = ConditionalAccess.Utils.ConvertToTstvRecordingStatus(recordingInternalStatus,
                oldRecording.AbsoluteStartTime ??
                programByEpg.StartDate.AddMinutes(-1 * oldRecording.PaddingBeforeMins),
                oldRecording.AbsoluteEndTime ?? programByEpg.EndDate.AddMinutes(oldRecording.PaddingAfterMins),
                oldRecording.CreateDate);

            if (recordingStatus == null || recordingStatus.Equals(TstvRecordingStatus.Scheduled))
            {
                return new Status(eResponseStatus.RecordingStatusNotValid,
                    $"Stop action is not allowed for recordings that are in status {TstvRecordingStatus.Scheduled}");
            }

            return new Status(eResponseStatus.OK);
        }

        public Status DeleteEpgEvent(int groupId, long epgId) //AfterIngest
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Program program = _repository.GetProgramByEpg(groupId, epgId);

            if (program == null)
            {
                return new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            var recordings = _repository.GetRecordingsByEpgId(groupId, epgId);

            int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
            string externalChannelId = null;

            foreach (var recording in recordings)
            {
                if (string.IsNullOrEmpty(externalChannelId))
                {
                    externalChannelId = CatalogDAL.GetEPGChannelCDVRId(groupId, recording.EpgChannelId);
                }

                //Todo - Matan use abs end if needed

                bool delete = program.EndDate.AddMinutes(recording.PaddingAfterMins) < DateTime.UtcNow;
                status = DeleteRecordingAdapter(groupId, recording.ExternalId, epgId, externalChannelId, adapterId,
                    delete);

                if (!status.IsOkStatusCode())
                {
                    return status;
                }

                status = InternalModifyRecording(groupId, recording.Id, recording.Key,
                    program.EndDate.AddMinutes(recording.PaddingAfterMins), true);
            }

            RecordingsUtils.UpdateIndex(groupId, program.Id, eAction.Delete);
            RecordingsUtils.UpdateCouchbase(groupId, epgId, program.Id, true);

            return status;
        }

        private Status DeleteRecordingAdapter(int groupId, string externalRecordingId, long epgId,
            string externalChannelId, int adapterId, bool delete)
        {
            Status status = new Status();

            RecordResult adapterResponse = null;
            var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

            try
            {
                if (delete)
                {
                    adapterResponse = adapterController.DeleteRecording(groupId, epgId.ToString(),
                        externalChannelId, externalRecordingId, adapterId, null);
                }
                else
                {
                    adapterResponse = adapterController.CancelRecording(groupId, epgId.ToString(),
                        externalChannelId, externalRecordingId, adapterId, 0);
                }
            }
            catch (KalturaException ex)
            {
                status = new Status((int)eResponseStatus.Error,
                    string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
                return status;
            }
            catch (Exception ex)
            {
                status = new Status((int)eResponseStatus.Error, "Adapter controller exception: " + ex.Message);
                return status;
            }

            return status;
        }

        public Status UpdateRecordingAfterIngest(int partnerId, long programId, DateTime startDate, DateTime endDate)
        {
            Status status = new Status();

            // If there is no recording, nothing to do
            Program program = _repository.GetProgramByEpg(partnerId, programId);
            if (program == null)
            {
                status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                return status;
            }

            // First of all - if EPG was updated, update the recording index, nevermind what was the change
            RecordingsUtils.UpdateCouchbase(partnerId, programId, program.Id);
            RecordingsUtils.UpdateIndex(partnerId, program.Id, eAction.Update);

            // If no change was made to program schedule - do nothing
            if (program.StartDate == startDate &&
                program.EndDate == endDate)
            {
                // We're OK
                status = new Status((int)eResponseStatus.OK);
                return status;
            }

            bool startTimeUpdated = program.StartDate != startDate;
            bool endTimeUpdated = program.EndDate != endDate;


            var _programOldStart = program.StartDate;
            var _programOldEnd = program.EndDate;
            program.StartDate = startDate;
            program.EndDate = endDate;

            //update program
            bool updated = _repository.UpdateProgram(partnerId, program);

            bool shouldUpdateDomainsQuota = false;

            int oldProgramRecordingLength = (int)(_programOldEnd - _programOldStart).TotalSeconds;
            int newProgramRecordingLength = (int)(endDate - startDate).TotalSeconds;

            shouldUpdateDomainsQuota = oldProgramRecordingLength != newProgramRecordingLength;

            var recordings = _repository.GetRecordingsByEpgId(partnerId, programId);

            int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(partnerId);
            var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();
            RecordResult adapterResponse = null;
            string externalChannelId = null;

            foreach (var recording in recordings)
            {
                Status recordingStatus = new Status();

                if (string.IsNullOrEmpty(externalChannelId))
                {
                    externalChannelId = CatalogDAL.GetEPGChannelCDVRId(partnerId, recording.EpgChannelId);
                }

                if (recording.AbsoluteStartTime.HasValue) //is immediate
                {
                    if (recording.AbsoluteStartTime.Value > endDate || recording.AbsoluteEndTime.Value < startDate)
                    {
                        if (!DeleteRecordingAdapter(partnerId, recording.ExternalId, recording.EpgId, externalChannelId,
                                adapterId, true).IsOkStatusCode())
                        {
                            log.Error(
                                $"Failed to remove recording: {recording.Key} from adapter: {adapterId}, recording external Id: {recording.ExternalId}, epgId: {recording.EpgId}");
                        }

                        if (_repository.UpdateRecordingStatus(partnerId, recording.Id,
                                RecordingInternalStatus.Failed.ToString()))
                        {
                            status = new Status((int)eResponseStatus.OK);

                            HandleFailedRecording(partnerId, recording); //delete and clear cache
                        }
                    }

                    continue;
                }

                var oldRecordingLengthSeconds = (int)Math.Ceiling(
                    oldProgramRecordingLength + TimeSpan
                        .FromMinutes(recording.PaddingBeforeMins + recording.PaddingAfterMins).TotalSeconds);

                bool shouldRetry = true;
                bool shouldMarkAsFailed = true;

                var recordingStartDate = startDate.AddMinutes(-1 * recording.PaddingBeforeMins);
                var recordingEndDate = endDate.AddMinutes(recording.PaddingAfterMins);

                try
                {
                    long startTimeSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(recordingStartDate);
                    long durationSeconds = (long)(recordingEndDate - recordingStartDate).TotalSeconds;

                    adapterResponse = adapterController.UpdateRecordingSchedule(partnerId, programId.ToString(),
                        externalChannelId, recording.ExternalId,
                        adapterId, startTimeSeconds, durationSeconds);
                }
                catch (KalturaException ex)
                {
                    recordingStatus = new Status((int)eResponseStatus.Error,
                        string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
                }
                catch (Exception ex)
                {
                    recordingStatus = new Status((int)eResponseStatus.Error,
                        "Adapter controller exception: " + ex.Message);
                }

                try
                {
                    var newRecordingInternalStatus = RecordingInternalStatus.Waiting;

                    if (adapterResponse != null)
                    {
                        // Set external recording ID
                        recording.ExternalId = adapterResponse.RecordingId;
                    }
                    else
                    {
                        shouldRetry = true;
                    }

                    // if adapter failed - retry AND mark as failed
                    // this is because we can't tell the recording is "Fine" when it is far from being fine
                    // its airing time has changed but the provider isn't aware of this... 
                    // so we must inform the users that their recording is not OK
                    if (recordingStatus != null && recordingStatus.Code != (int)eResponseStatus.OK)
                    {
                        shouldMarkAsFailed = true;
                        shouldRetry = true;
                    }

                    // If we have a response AND we didn't set the status to be invalid
                    if (adapterResponse != null &&
                        (recordingStatus == null || recordingStatus.Code == (int)eResponseStatus.OK))
                    {
                        // if provider failed
                        if (!adapterResponse.ActionSuccess || adapterResponse.FailReason != 0)
                        {
                            shouldRetry = true;
                            shouldMarkAsFailed = true;
                        }
                        else
                        {
                            var tstvRecordingStatus = RecordingsUtils.GetTstvRecordingStatus(recordingStartDate,
                                recordingEndDate, TstvRecordingStatus.Scheduled);
                            recording.SetStatus(tstvRecordingStatus);

                            // everything is good
                            shouldMarkAsFailed = false;
                            shouldRetry = false;

                            newRecordingInternalStatus = RecordingInternalStatus.OK;
                        }
                    }

                    if (shouldMarkAsFailed)
                    {
                        recording.SetStatus(TstvRecordingStatus.Failed);
                        newRecordingInternalStatus = RecordingInternalStatus.Failed;
                    }

                    // Update the result from the adapter
                    bool updateSuccess = UpdateRecording(partnerId, program, recording);

                    if (!updateSuccess)
                    {
                        recordingStatus = new Status((int)eResponseStatus.Error,
                            "Failed updating recording in database.");
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Failed updating recording {0} in database and queue: {1}", recording.Id, ex);
                    recordingStatus = new Status((int)eResponseStatus.Error,
                        "Failed inserting/updating recording in database and queue.");
                }

                if (shouldRetry)
                {
                    RetryTaskBeforeProgramStarted(partnerId, recordingStartDate, recording);
                }

                if (shouldUpdateDomainsQuota && status.Code == (int)eResponseStatus.OK)
                {
                    try
                    {
                        SendEvictRecording(partnerId, recording.Id, oldRecordingLengthSeconds,
                            DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow));
                    }
                    catch (Exception e)
                    {
                        log.Error($"Failed to queue task in UpdateRecording, recording: {recording}", e);
                    }
                }
            }

            return status;
        }

        public bool UpdateRecording(int groupId, Program program, TimeBasedRecording recording)
        {
            TimeShiftedTvPartnerSettings accountSettings =
                ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);

            int? recordingLifetime = accountSettings.RecordingLifetimePeriod;

            //BEO - 7188
            if (recordingLifetime.HasValue)
            {
                DateTime viewableUntilDate = program.EndDate.AddDays(recordingLifetime.Value);
                recording.ViewableUntilEpoch =
                    TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(viewableUntilDate);
                recording.UpdateDate = DateTime.UtcNow;
            }

            return _repository.UpdateRecording(groupId, recording);
        }

        private HouseholdRecording UpdateHouseholdRecording(int partnerId, long hhRecordingId, long householdId,
            string oldKey, string newKey, string oldStatus, string newStatus = null)
        {
            var currentHhRecording = _repository.GetHouseholdRecording(partnerId, oldKey, householdId);
            if (currentHhRecording == null)
                return null;

            /*Modify current*/
            currentHhRecording.RecordingKey = newKey;
            currentHhRecording.Status = newStatus ?? oldStatus;

            if (!_repository.UpdateHouseholdRecording(partnerId, currentHhRecording))
            {
                log.Error($"Couldn't update hh recording: {hhRecordingId}");
                return null;
            }

            return _repository.GetHouseholdRecording(partnerId, newKey, householdId);
        }

        public Recording UpdateRecordingByHousehold(int partnerId, long hhRecordingId, long householdId,
            long protectedUntilEpoch, long userId, int? paddingAfterMins, int? paddingBeforeMins,
            long? viewableUntilDate)
        {
            HouseholdRecording currentHhRecording =
                _repository.GetHouseholdRecording(partnerId, hhRecordingId, householdId);
            TimeBasedRecording timeBasedRecording =
                _repository.GetRecordingByKey(partnerId, currentHhRecording.RecordingKey);
            List<HouseholdRecording> hhRecording =
                _repository.GetTop2HouseholdRecordingsByKey(partnerId, timeBasedRecording.Key);
            Program program = _repository.GetProgramByEpg(partnerId, timeBasedRecording.EpgId);
            Recording recording =
                RecordingsUtils.BuildRecordingFromTBRecording(partnerId, timeBasedRecording, program, currentHhRecording);

            if (timeBasedRecording.PaddingAfterMins != paddingAfterMins ||
                timeBasedRecording.PaddingBeforeMins != paddingBeforeMins ||
                (timeBasedRecording.IsImmediateRecording() && paddingAfterMins.HasValue &&
                 timeBasedRecording.AbsoluteEndTime != program.EndDate.AddMinutes(paddingAfterMins.Value)
                     .RoundUp(TimeSpan.FromMinutes(1))))
            {
                RecordResult adapterResponse = null;
                timeBasedRecording.PaddingAfterMins =
                    (paddingAfterMins.HasValue /*&& !timeBasedRecording.IsImmediateRecording()*/)
                        ? paddingAfterMins.Value
                        : timeBasedRecording.PaddingAfterMins;
                timeBasedRecording.PaddingBeforeMins =
                    (paddingBeforeMins.HasValue && !timeBasedRecording.IsImmediateRecording())
                        ? paddingBeforeMins.Value
                        : timeBasedRecording.PaddingBeforeMins;
                timeBasedRecording.AbsoluteEndTime =
                    paddingAfterMins.HasValue && timeBasedRecording.IsImmediateRecording()
                        ? program.EndDate.AddMinutes(paddingAfterMins.Value).RoundUp(TimeSpan.FromMinutes(1))
                        : timeBasedRecording.AbsoluteEndTime;
                if (viewableUntilDate.HasValue)
                    timeBasedRecording.ViewableUntilEpoch = viewableUntilDate.Value;

                string recordingKey;
                if (timeBasedRecording.IsImmediateRecording())
                {
                    var absoluteStartTimeEpoch =
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(timeBasedRecording.AbsoluteStartTime);
                    var absoluteEndTimeEpoch =
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(timeBasedRecording.AbsoluteEndTime);
                    recordingKey = GetImmediateRecordingKey(timeBasedRecording.EpgId, absoluteStartTimeEpoch,
                        absoluteEndTimeEpoch);
                }
                else
                {
                    recordingKey = GetRecordingKey(timeBasedRecording.EpgId,
                        timeBasedRecording.PaddingBeforeMins, timeBasedRecording.PaddingAfterMins);
                }

                if (hhRecording.Count == 1)
                {
                    timeBasedRecording.Key = recordingKey;
                    currentHhRecording.RecordingKey = recordingKey;
                    currentHhRecording.ProtectedUntilEpoch = protectedUntilEpoch;

                    DateTime recordingStartDate =
                        GetStartDateWithPadding(program.StartDate, timeBasedRecording.PaddingBeforeMins);
                    DateTime recordingEndDate = program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins);
                    long startTimeSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(recordingStartDate);
                    long durationSeconds = (long)(recordingStartDate - recordingEndDate).TotalSeconds;
                    var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();
                    int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(partnerId);
                    string externalChannelId =
                        CatalogDAL.GetEPGChannelCDVRId(partnerId, timeBasedRecording.EpgChannelId);
                    
                    if (_repository.UpdateRecording(partnerId, timeBasedRecording))
                    {
                        _repository.UpdateHouseholdRecording(partnerId, currentHhRecording);
                        LayeredCache.Instance.SetInvalidationKey(
                            LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId, householdId));
                        adapterResponse = adapterController.UpdateRecordingSchedule(partnerId,
                            timeBasedRecording.EpgId.ToString(), externalChannelId,
                            timeBasedRecording.ExternalId, adapterId, startTimeSeconds, durationSeconds);
                        if (adapterResponse.ActionSuccess)
                        {
                            recording.EndPadding = timeBasedRecording.PaddingAfterMins;
                            recording.StartPadding = timeBasedRecording.PaddingBeforeMins;
                            recording.ProtectedUntilDate = protectedUntilEpoch;
                            recording.AbsoluteEndTime = timeBasedRecording.AbsoluteEndTime;
                        }
                    }
                }
                else
                {
                    _repository.DeleteHouseholdRecording(partnerId, hhRecording[0].RecordingKey,
                        hhRecording[0].HouseholdId);
                    HashSet<long> failedDomainIds;
                    recording = Record(partnerId, timeBasedRecording.EpgId, timeBasedRecording.EpgChannelId,
                        program.StartDate, program.EndDate,
                        timeBasedRecording.Crid, new List<long>() { householdId }, out failedDomainIds,
                        paddingBeforeMins, paddingAfterMins);

                    if (UpdateOrInsertHouseholdRecording(partnerId, userId, householdId, recording,
                            recordingKey, TstvRecordingStatus.OK, false))
                    {
                        LayeredCache.Instance.SetInvalidationKey(
                            LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId, householdId));
                    }
                }
            }
            else if (currentHhRecording.ProtectedUntilEpoch != protectedUntilEpoch)
            {
                currentHhRecording.ProtectedUntilEpoch = protectedUntilEpoch;
                _repository.UpdateHouseholdRecording(partnerId, currentHhRecording);
                LayeredCache.Instance.SetInvalidationKey(
                    LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId, householdId));
            }

            return recording;
        }

        private DateTime GetStartDateWithPadding(DateTime startDate, long min)
        {
            return startDate.AddMinutes((-1) * min);
        }

        public Status RecordRetry(int groupId, long recordingId)
        {
            Status returnStatus = new Status((int)eResponseStatus.OK);
            TimeBasedRecording timeBasedRecording = _repository.GetRecordingById(groupId, recordingId);
            Program program = _repository.GetProgramByEpg(groupId, timeBasedRecording.EpgId);
            Recording recording = RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, program);
            try
            {
                // If couldn't find a recording with this ID
                if (timeBasedRecording.Id == 0)
                {
                    string message = string.Format("Retry Record - could not find Recording with Id = {0} in group {1}",
                        recordingId, groupId);
                    log.Error(message);
                    returnStatus = new Status((int)eResponseStatus.OK, message);
                }
                else
                {
                    // if this program is in the past (because it moved, for example)
                    if (GetStartDateWithPadding(program.StartDate, timeBasedRecording.PaddingBeforeMins) <
                        DateTime.UtcNow)
                    {
                        string message =
                            string.Format("Retry Record - Recording with Id = {0} in group {1} is already in the past",
                                recordingId, groupId);
                        log.Error(message);
                        returnStatus = new Status((int)eResponseStatus.OK, message);
                    }
                    else
                    {
                        List<long> domainIds = new List<long>();
                        HashSet<long> failedDomainIds;

                        RecordingContext recordingContext = RecordingContext.Regular;


                        //BEO-9046 - get domainIds + paging 500
                        if (ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId).IsPrivateCopyEnabled.Value)
                        {
                            //   domainIds = RecordingsDAL.GetDomainsEpgRecordingFailure(groupId, recording.EpgId);
                            domainIds = _repository.GetHhRecordingsFailuresByKey(groupId, timeBasedRecording.Key);
                            if (domainIds.Count == 0)
                            {
                                returnStatus = new Status((int)eResponseStatus.OK);
                                return returnStatus;
                            }

                            recordingContext = RecordingContext.PrivateRetry;

                            //BEO-9298/9302, mark as deleted and remove from all domains
                            if (timeBasedRecording.Id == 0 ||
                                !RecordingsUtils.IsValidRecordingStatus(recording.RecordingStatus))
                            {
                                var state = RecordingsUtils.ConvertToDomainRecordingStatus(recording.RecordingStatus) ??
                                            DomainRecordingStatus.Failed;
                                var affectedHouseholds =
                                    _repository.UpdateHhRecordingsStatusByRecordingKey(groupId, timeBasedRecording.Key,
                                        state.ToString());
                                if (affectedHouseholds.Count > 0)
                                {
                                    affectedHouseholds.ForEach(household =>
                                        LayeredCache.Instance.SetInvalidationKey(
                                            LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(groupId,
                                                household.Id)));
                                }

                                return returnStatus;
                            }

                            int index = 0;
                            var ids = new List<long>();
                            do
                            {
                                ids = domainIds.Skip(index * 500).Take(500).ToList();
                                CallAdapterRecord(groupId, timeBasedRecording.EpgId.ToString(),
                                    timeBasedRecording.EpgChannelId,
                                    program.StartDate.AddMinutes(timeBasedRecording.PaddingBeforeMins),
                                    program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins), timeBasedRecording,
                                    ids, out failedDomainIds, recordingContext);
                                index++;
                            } while (ids.Count > 0);
                        }
                        else
                        {
                            CallAdapterRecord(groupId, timeBasedRecording.EpgId.ToString(),
                                timeBasedRecording.EpgChannelId,
                                program.StartDate.AddMinutes(timeBasedRecording.PaddingBeforeMins),
                                program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins), timeBasedRecording,
                                domainIds, out failedDomainIds, recordingContext);
                        }

                        // If we got through here without any exception, we're ok.
                        returnStatus = new Status((int)eResponseStatus.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                returnStatus = new Status(
                    (int)eResponseStatus.Error,
                    string.Format("Record retry failure: id = {0}, ex = {1}", recordingId, ex));
            }

            return returnStatus;
        }

        public Status GetRecordingStatus(int partnerId, long recordingId)
        {
            Status returnStatus = new Status((int)eResponseStatus.OK);
            TimeBasedRecording timeBasedRecording = _repository.GetRecordingById(partnerId, recordingId);
            if (timeBasedRecording != null)
            {
                Program program = _repository.GetProgramByEpg(partnerId, timeBasedRecording.EpgId);

                try
                {
                    if (timeBasedRecording.Id == 0)
                    {
                        log.ErrorFormat("GetRecordingStatus: no recording with ID = {0}", recordingId);
                        returnStatus = new Status((int)eResponseStatus.OK,
                            string.Format("No recording with ID = {0} found", recordingId));
                    }
                    else
                    {
                        // If the program finished already or not: if it didn't finish, then the recording obviously didn't finish...
                        if (program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins) < DateTime.UtcNow)
                        {
                            var timeSpan = DateTime.UtcNow -
                                           program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins);
                            TstvRecordingStatus paddedRecordingStatus =
                                (TstvRecordingStatus)Enum.Parse(typeof(TstvRecordingStatus), timeBasedRecording.Status);
                            // If this recording is mark as failed, there is no point in trying to get its status
                            if (paddedRecordingStatus == TstvRecordingStatus.Failed)
                            {
                                log.InfoFormat(
                                    "Rejected GetRecordingStatus request because it is already failed. recordingId = {0}" +
                                    "minutesSpan = {1}, allowedDifferenceMinutes = {2}, retryCount = {3}, epg = {4}",
                                    recordingId, timeSpan.TotalMinutes, MINUTES_ALLOWED_DIFFERENCE,
                                    timeBasedRecording.RetriesStatus, program.EpgId);
                            }
                            // Only if this is the first request and the difference is less than 5 minutes we continue
                            // If this is not the first request, we're clear to go even if it is far from being after the program ended
                            else if ((timeBasedRecording.RetriesStatus == 0 &&
                                      timeSpan.TotalMinutes < MINUTES_ALLOWED_DIFFERENCE) ||
                                     timeBasedRecording.RetriesStatus > 0)
                            {
                                // Count current try to get status - first and foremost
                                timeBasedRecording.RetriesStatus++;

                                DateTime nextCheck = DateTime.UtcNow.AddMinutes(MINUTES_RETRY_INTERVAL);

                                if (string.IsNullOrEmpty(timeBasedRecording.ExternalId))
                                {
                                    ///BEO-9708
                                    log.Debug(
                                        $"GetRecordingStatus: (BEO-9708) ExternalRecordingId is empty! recordingId:{recordingId}");

                                    if (timeSpan.TotalMinutes < MINUTES_ALLOWED_DIFFERENCE)
                                    {
                                        timeBasedRecording.Status = RecordingsUtils
                                            .ConvertToRecordingInternalStatus(paddedRecordingStatus).ToString();
                                        UpdateRecording(partnerId, program, timeBasedRecording);
                                        RetryTaskAfterProgramEnded(partnerId, timeBasedRecording, program, nextCheck,
                                            eRecordingTask.GetStatusAfterProgramEnded);
                                    }
                                    else
                                    {
                                        log.Debug(
                                            $"GetRecordingStatus: (BEO-9708) ExternalRecordingId is empty! set to Failed. recordingId:{recordingId}");
                                        timeBasedRecording.Status = RecordingInternalStatus.Failed.ToString();
                                        returnStatus = new Status((int)eResponseStatus.Error, "no ExternalRecordingId");
                                        UpdateRecording(partnerId, program, timeBasedRecording);
                                        RecordingsUtils.UpdateIndex(partnerId, program.Id, eAction.Update);
                                    }

                                    returnStatus.Code = (int)eResponseStatus.OK;
                                    return returnStatus;
                                }

                                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(partnerId);

                                var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                                RecordResult adapterResponse = null;
                                string externalChannelId =
                                    CatalogDAL.GetEPGChannelCDVRId(partnerId, timeBasedRecording.EpgChannelId);

                                try
                                {
                                    adapterResponse = adapterController.GetRecordingStatus(partnerId, externalChannelId,
                                        timeBasedRecording.ExternalId, adapterId);
                                }
                                catch (KalturaException ex)
                                {
                                    log.ErrorFormat(
                                        "GetRecordingStatus: KalturaException when using adapter. ID = {0}, ex = {1}, message = {2}, code = {3}",
                                        recordingId, ex, ex.Message, ex.Data["StatusCode"]);
                                    adapterResponse = null;
                                }
                                catch (Exception ex)
                                {
                                    log.ErrorFormat(
                                        "GetRecordingStatus: Exception when using adapter. ID = {0}, ex = {1}",
                                        recordingId, ex);
                                    adapterResponse = null;
                                }

                                // Adapter failed for some reason - retry
                                if (adapterResponse == null)
                                {
                                    paddedRecordingStatus = (TstvRecordingStatus)Enum.Parse(typeof(TstvRecordingStatus),
                                        timeBasedRecording.Status);
                                    timeBasedRecording.Status = RecordingsUtils
                                        .ConvertToRecordingInternalStatus(paddedRecordingStatus).ToString();
                                    UpdateRecording(partnerId, program, timeBasedRecording);
                                    RetryTaskAfterProgramEnded(partnerId, timeBasedRecording, program, nextCheck,
                                        eRecordingTask.GetStatusAfterProgramEnded);
                                }
                                else
                                {
                                    // If it was successfull - we mark it as recorded
                                    if (adapterResponse.ActionSuccess && adapterResponse.FailReason == 0)
                                    {
                                        timeBasedRecording.Status = TstvRecordingStatus.Recorded.ToString();
                                    }
                                    else
                                    {
                                        timeBasedRecording.Status = TstvRecordingStatus.Failed.ToString();
                                        returnStatus = new Status((int)eResponseStatus.Error,
                                            $"Adapter failed for reason: {adapterResponse.FailReason}." +
                                            $" Provider code = {adapterResponse.ProviderStatusCode}, provider message = {adapterResponse.ProviderStatusMessage}, fail reason = {adapterResponse.FailReason}");
                                    }

                                    // Update recording after setting the new status
                                    paddedRecordingStatus = (TstvRecordingStatus)Enum.Parse(typeof(TstvRecordingStatus),
                                        timeBasedRecording.Status);
                                    timeBasedRecording.Status = RecordingsUtils
                                        .ConvertToRecordingInternalStatus(paddedRecordingStatus).ToString();
                                    UpdateRecording(partnerId, program, timeBasedRecording);
                                    RecordingsUtils.UpdateIndex(partnerId, program.Id, eAction.Update);
                                }
                            }
                            else
                            {
                                log.InfoFormat(
                                    "Rejected GetRecordingStatus request because it is too far from the end of the program. recordingId = {0}" +
                                    "minutesSpan = {1}, allowedDifferenceMinutes = {2}, retryCount = {3}, epg = {4}",
                                    recordingId, timeSpan.TotalMinutes, MINUTES_ALLOWED_DIFFERENCE,
                                    timeBasedRecording.RetriesStatus, program.EpgId);
                            }
                        }
                    }

                    // if we didn't have any exception, mark as success, because we don't want the remote task to call CAS again
                    // CAS should call GetRecordingStatus only if this method failed completely.
                    // Adapter failure doesn't mean an immediate retry!
                    TstvRecordingStatus recordingStatus =
                        (TstvRecordingStatus)Enum.Parse(typeof(TstvRecordingStatus), timeBasedRecording.Status);

                    if (recordingStatus == TstvRecordingStatus.Recorded)
                    {
                        HandleSuccessRecording(partnerId, timeBasedRecording, program.StartDate);
                    }
                    else if (recordingStatus == TstvRecordingStatus.Failed)
                    {
                        HandleFailedRecording(partnerId, timeBasedRecording);
                    }
                }
                catch (Exception ex)
                {
                    returnStatus = new Status((int)eResponseStatus.Error,
                        string.Format("Exception {0}", ex));
                }
            }

            return returnStatus;
        }

        public void HandleSuccessRecording(int partnerId, TimeBasedRecording recording, DateTime startDate)
        {
            Dictionary<long, long>
                userDomainDictionary = new Dictionary<long, long>(); // fill with users with no Entitlement
            List<HouseholdRecording> householdRecordings =
                _repository.GetHhRecordingsByKey(partnerId, recording.Key, DomainRecordingStatus.OK.ToString());

            // look for all users asked for this recordingId as SINGLE recording 

            long epgId = recording.EpgId;
            //Check user Entitled for the channel 
            //Updated definitions for future/scheduled single recordings on channel entitlements revoke � to allow lazy removal
            List<EPGChannelProgrammeObject> epgs =
                ConditionalAccess.Utils.GetEpgsByIds(partnerId, new List<long>() { epgId });
            if (epgs == null || epgs.Count == 0)
            {
                log.DebugFormat("Failed Getting EPGs from Catalog, recordingId: {0}, epgId: {1}", recording.Id, epgId);
                return;
            }

            foreach (HouseholdRecording householdRecording in householdRecordings)
            {
                bool isUserAddToDic = false;

                // domain not allowed to service
                if (!RecordingsUtils.IsServiceAllowed(partnerId, (int)householdRecording.HouseholdId, eService.NPVR))
                {
                    if (!userDomainDictionary.ContainsKey(householdRecording.UserId))
                    {
                        userDomainDictionary.Add(householdRecording.UserId, householdRecording.HouseholdId);
                        isUserAddToDic = true;
                    }
                }
                else // validate epgs entitlement and add to response
                {
                    Recording validatedRecording = RecordingsUtils.ValidateEntitlementForEpg(partnerId,
                        householdRecording.UserId, householdRecording.HouseholdId, epgs[0]);
                    if (validatedRecording.Status.Code == (int)eResponseStatus.NotEntitled)
                    {
                        if (!userDomainDictionary.ContainsKey(householdRecording.UserId))
                        {
                            userDomainDictionary.Add(householdRecording.UserId, householdRecording.HouseholdId);
                            isUserAddToDic = true;
                        }
                    }
                }

                if (!isUserAddToDic)
                {
                    LayeredCache.Instance.SetInvalidationKey(
                        LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId,
                            householdRecording.HouseholdId));
                }
            }

            // get all other recording in status SCHEDULED for the non entitled users
            if (userDomainDictionary.Count > 0) // some domains not entitled to channel
            {
                DomainRecordingStatus? domainRecordingStatus =
                    RecordingsUtils.ConvertToDomainRecordingStatus(TstvRecordingStatus.Scheduled);

                List<HouseholdRecording> hhRecordings = _repository.GetHhRecordingsByChannelID(partnerId,
                    recording.EpgChannelId, domainRecordingStatus.ToString(), RecordingType.Single.ToString());
                List<long> epgIdIds = hhRecordings.Select(e => e.EpgId).ToList();
                List<Program> programs = _repository.GetProgramsByEpgIdIdsAndStartDate(partnerId, epgIdIds, startDate);

                if (hhRecordings.Count > 0)
                {
                    //cancel all of those + the record with the epgid we got before 
                    // set max amount of concurrent tasks
                    int maxDegreeOfParallelism =
                        ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
                    if (maxDegreeOfParallelism == 0)
                    {
                        maxDegreeOfParallelism = 5;
                    }

                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                    LogContextData contextData = new LogContextData();
                    Parallel.ForEach(hhRecordings.AsEnumerable(), options, (hhRecording) =>
                    {
                        Program program = programs.Find(x => x.EpgId == hhRecording.EpgId);
                        if (program != null)
                        {
                            TstvRecordingStatus currentTstv = hhRecording.RecordingKey == recording.Key
                                ? TstvRecordingStatus.Deleted
                                : TstvRecordingStatus.Canceled;
                            RecordingsUtils.CancelOrDeleteRecord(partnerId, hhRecording.UserId.ToString(),
                                hhRecording.HouseholdId, hhRecording.Id, currentTstv, false);
                        }
                    });
                }
            }
        }

        public void HandleFailedRecording(int partnerId, TimeBasedRecording recording)
        {
            try
            {
                List<HouseholdRecording> householdRecordings =
                    _repository.GetHhRecordingsByKey(partnerId, recording.Key, DomainRecordingStatus.OK.ToString());

                // set max amount of concurrent tasks
                int maxDegreeOfParallelism = ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
                if (maxDegreeOfParallelism == 0)
                {
                    maxDegreeOfParallelism = 5;
                }

                var domainRecordingIds = householdRecordings.Select(r => r.Id).ToList();
                var updatedHouseholdRecordings = _repository.UpdateHouseholdRecordingsStatus(partnerId,
                    domainRecordingIds,
                    DomainRecordingStatus.Failed.ToString());

                if (!householdRecordings.Count.Equals(updatedHouseholdRecordings.Count))
                {
                    log.Warn(
                        $"Failed to update HH recording with status Failure for some households, RecordingId:{recording.Id}");
                }

                ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };

                LogContextData contextData = new LogContextData();
                Parallel.For(0, householdRecordings.Count, options, i =>
                {
                    contextData.Load();
                    long householdId = householdRecordings[i].HouseholdId;
                    if (householdId > 0)
                    {
                        LayeredCache.Instance.SetInvalidationKey(
                            LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId, householdId));
                        // if (!CompleteDomainSeriesRecordings(domainId))
                        // {
                        //     log.ErrorFormat("Failed CompleteHouseholdSeriesRecordings after modifiedRecordingId: {0}, for domainId: {1}", recording.Id, domainId);
                        // }
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Exception HandleFailedRecording, RecordingId:{0}", recording.Id), ex);
            }
        }

        private void RetryTaskAfterProgramEnded(int partnerId, TimeBasedRecording timeBasedRecording, Program program,
            DateTime nextCheck, eRecordingTask recordingTask)
        {
            // Retry in a few minutes if we still didn't exceed retries count
            if (timeBasedRecording.RetriesStatus <= MAXIMUM_RETRIES_ALLOWED)
            {
                log.DebugFormat(
                    "Try to enqueue retry task: groupId {0}, recordingId {1}, nextCheck {2}, recordingTask {3}, retries {4}",
                    partnerId, timeBasedRecording.Id, nextCheck.ToString(), recordingTask.ToString(),
                    timeBasedRecording.RetriesStatus);

                SendVerifyRecordingFinalStatus(partnerId, timeBasedRecording.Id, nextCheck);
            }
            else
            {
                log.DebugFormat(
                    "Exceeded allowed retried count, trying to mark as failed: groupId {0}, recordingId {1}, nextCheck {2}, recordingTask {3}, retries {4}",
                    partnerId, timeBasedRecording.Id, nextCheck.ToString(), recordingTask.ToString(),
                    timeBasedRecording.RetriesStatus);

                // Otherwise, we tried too much! Mark this recording as failed. Sorry mates!
                timeBasedRecording.Status = RecordingInternalStatus.Failed.ToString();
                // Update recording after updating the status
                UpdateRecording(partnerId, program, timeBasedRecording);

                try
                {
                    SendEvictRecording(partnerId, timeBasedRecording.Id, 0,
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow));
                }
                catch (Exception e)
                {
                    log.Error(
                        $"Failed to queue ExpiredRecording task for RetryTaskAfterProgramEnded when recording FAILED, recordingId: {timeBasedRecording.Id}, groupId: {partnerId}",
                        e);
                }
            }
        }

        public List<Program> GetProgramsByProgramIds(int partnerId, List<long> programIds)
        {
            return _repository.GetProgramsByProgramIds(partnerId, programIds);
        }

        public void RetryTaskBeforeProgramStarted(int partnerId, DateTime recordingStartDate,
            TimeBasedRecording currentRecording)
        {
            var span = recordingStartDate - DateTime.UtcNow;

            DateTime nextCheck;

            // if there is more than 1 day left, try tomorrow
            if (span.TotalDays > 1)
            {
                log.DebugFormat(
                    "Retry task before program started: Recording id = {0} will retry tomorrow, because start date is {1}",
                    currentRecording.Id, recordingStartDate);

                nextCheck = DateTime.UtcNow.AddDays(1);
            }
            else if (span.TotalHours > 1)
            {
                log.DebugFormat(
                    "Retry task before program started: Recording id = {0} will retry in half the time (in {1} hours), because start date is {2}",
                    currentRecording.Id, (span.TotalHours / 2), recordingStartDate);

                // if there is less than 1 day, get as HALF as close to the start of the program.
                // e.g. if we are 4 hours away from program, check in 2 hours. If we are 140 minutes away, try in 70 minutes.
                nextCheck = DateTime.UtcNow.AddSeconds(span.TotalSeconds / 2);
            }
            else
            {
                log.DebugFormat(
                    "Retry task before program started: Recording id = {0} will retry when program starts, at {1}",
                    currentRecording.Id, recordingStartDate);

                // if we are less than an hour away from the program, try when the program starts (half a minute before it starts)
                nextCheck = recordingStartDate.AddSeconds(-30);
            }

            bool isPrivateCopy = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(partnerId).IsPrivateCopyEnabled
                .Value;

            // continue checking until the program started. 
            if (DateTime.UtcNow < recordingStartDate &&
                DateTime.UtcNow < nextCheck)
            {
                if (isPrivateCopy)
                {
                    var _nextCheck = RecordingsDAL.GetDomainRetryRecordingDoc(partnerId, currentRecording.EpgId);
                    var next = DateUtils.ToUtcUnixTimestampSeconds(nextCheck);

                    if (!_nextCheck.HasValue || (_nextCheck.HasValue &&
                                                 _nextCheck.Value <= DateUtils.GetUtcUnixTimestampNow())
                                             || _nextCheck.Value > next)
                    {
                        log.Debug($"Updating retry document for epg: {currentRecording.EpgId}, group: {partnerId}");
                        //enqueue - only if no existing cb document
                        if (!_nextCheck.HasValue)
                        {
                            SendRetryRecording(partnerId, currentRecording.Id, nextCheck);
                        }

                        //Update document
                        RecordingsDAL.SaveDomainRetryRecordingDoc(partnerId, currentRecording.EpgId, next,
                            RecordingsUtils.CalcTtl(nextCheck));
                    }
                }
                else
                {
                    log.DebugFormat(
                        "Retry task before program started: program didn't start yet, we will enqueue a message now for recording {0}",
                        currentRecording.Id);
                    SendRetryRecording(partnerId, currentRecording.Id, nextCheck);
                }
            }
            else
                // If it is still not ok - mark as failed
            {
                if (!isPrivateCopy)
                {
                    log.DebugFormat(
                        "Retry task before program started: program started already, we will mark recording {0} as failed.",
                        currentRecording.Id);
                    currentRecording.Status = RecordingInternalStatus.Failed.ToString();

                    // Update recording after updating the status
                    _repository.UpdateRecording(partnerId, currentRecording);

                    // Update all domains that have this recording   
                    try
                    {
                        SendEvictRecording(partnerId, currentRecording.Id, 0,
                            DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow));
                    }
                    catch (Exception e)
                    {
                        log.Error(
                            $"Failed to queue ExpiredRecording task for RetryTaskAfterProgramEnded when recording FAILED, recordingId: {currentRecording.Id}, groupId: {partnerId}",
                            e);
                    }
                }
                else
                {
                    log.DebugFormat(
                        "Retry task before program started: program started already, Domain recording {0} is marked as failed.",
                        currentRecording.Id);
                    //BEO-9046 - Mark as failed for all failed domains by recording_id

                    var affectedHouseholds =
                        _repository.UpdateHhRecordingsStatusByRecordingKey(partnerId, currentRecording.Key,
                            DomainRecordingStatus.Failed.ToString());
                    if (affectedHouseholds.Count > 0)
                    {
                        affectedHouseholds.ForEach(household =>
                            LayeredCache.Instance.SetInvalidationKey(
                                LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId, household.Id)));
                    }
                }
            }
        }

        public GenericResponse<Recording> ImmediateRecord(int groupId, long userId, long domainId, long epgChannelId,
            long epgId, int? endPadding = null)
        {
            var response = new GenericResponse<Recording>();
            response.SetStatus(Status.Ok);

            var epgs = Core.ConditionalAccess.Utils.GetEpgsByIds(groupId, new List<long>() { epgId });
            if (epgs == null || epgs.Count == 0)
            {
                var _msg = $"Program {epgId} wasn't found";
                log.Debug(_msg);
                response.SetStatus(eResponseStatus.ProgramDoesntExist, _msg);
                return response;
            }
            
            //get program from epg
            var epgChannelProg = epgs.First();
            epgChannelProg.ParseDate(epgChannelProg.START_DATE, out var epgStartDate);
            epgChannelProg.ParseDate(epgChannelProg.END_DATE, out var epgEndDate);
            var _program = new Program(epgId, epgStartDate, epgEndDate);

            var accountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);

            var _absoluteStartTime = DateTime.UtcNow.RoundDown(TimeSpan.FromMinutes(1));
            var absoluteEndOffset = endPadding ?? Core.ConditionalAccess.Utils.ConvertSecondsToMinutes((int)(accountSettings.PaddingAfterProgramEnds ?? 0));
            var _absoluteEndTime =
                _program.EndDate.AddMinutes(absoluteEndOffset).RoundUp(TimeSpan.FromMinutes(1));

            if (DateTime.UtcNow < _program.StartDate)
            {
                var _msg = $"This program {epgId} is in the future";
                log.Debug(_msg);
                response.SetStatus(eResponseStatus.RecordingStatusNotValid, _msg);
                return response;
            }

            if (_absoluteStartTime >= _program.EndDate)
            {
                var _msg = $"This program {epgId} is in the past";
                log.Debug(_msg);
                response.SetStatus(eResponseStatus.RecordingStatusNotValid, _msg);
                return response;
            }

            var absoluteStartEpoch = DateUtils.DateTimeToUtcUnixTimestampSeconds(_absoluteStartTime);
            var absoluteEndEpoch = DateUtils.DateTimeToUtcUnixTimestampSeconds(_absoluteEndTime);

            var _recDates = new Recording()
            {
                EpgStartDate = _program.StartDate,
                EpgEndDate = _program.EndDate,
                AbsoluteStartTime = _absoluteStartTime,
                AbsoluteEndTime = _absoluteEndTime
            };

            if (!ValidateRecordingConcurrency(groupId, domainId, _recDates))
            {
                var _msg =
                    $"epgID: {_program.EpgId} can't be recoded due to recording concurrency of {accountSettings.MaxRecordingConcurrency}";
                log.Debug(_msg);
                response.SetStatus(eResponseStatus.RecordingExceededConcurrency, _msg);
                return response;
            }

            //Validate quota & entitlement
            var isQuotaOverage = RecordingsUtils.QuotaOverageAndEntitlement(groupId, domainId, userId, epgId,
                epgs.First(),
                absoluteStartEpoch, absoluteEndEpoch);

            if (!isQuotaOverage.Status.IsOkStatusCode())
            {
                response.SetStatus(isQuotaOverage.Status);
                return response;
            }

            Recording recording = null;
            TimeBasedRecording timeBasedRecording;
            var syncKey = string.Format("PaddedRecordingsManager_{0}", epgId);
            Dictionary<string, object> syncParmeters = new Dictionary<string, object>();
            syncParmeters.Add("groupId", groupId);
            syncParmeters.Add("programId", epgId);
            syncParmeters.Add("epgChannelID", epgChannelId);
            syncParmeters.Add("domainIds", new List<long>() { });
            syncParmeters.Add("isPrivateCopy", false);
            syncParmeters.Add("absoluteStart", _absoluteStartTime);
            syncParmeters.Add("absoluteEnd", _absoluteEndTime);
            syncParmeters.Add("startDate", _program.StartDate); 
            syncParmeters.Add("endDate", _program.EndDate); //calc viewable until date
            syncParmeters.Add("paddingAfter", absoluteEndOffset);
            syncParmeters.Add("crid", epgs.First().CRID);

            try
            {
                List<string> recordingStatuses = new List<string>()
                {
                    RecordingInternalStatus.OK.ToString(), RecordingInternalStatus.Waiting.ToString()
                };
                var hhRecordings = _repository.GetHhRecordingsByEpgId(groupId, domainId, epgId,
                    recordingStatuses, RecordingType.Single.ToString());
                if (hhRecordings != null && hhRecordings.Count > 0)
                {
                    var allEpgRecords = hhRecordings.ToList();
                    if (allEpgRecords.Count() >= MaxAllowedActiveRecordings)
                    {
                        response.SetStatus(eResponseStatus.RecordingStatusNotValid,
                            $"Can't record more than {MaxAllowedActiveRecordings} times from the same program");
                        return response;
                    }

                    //Validate on-going recording
                    foreach (var hhRecord in allEpgRecords)
                    {
                        var permutation = _repository.GetRecordingByKey(groupId, hhRecord.RecordingKey);
                        if (permutation != null && (!permutation.AbsoluteEndTime.HasValue ||
                                                    permutation.AbsoluteEndTime > DateTime.UtcNow))
                        {
                            response.SetStatus(eResponseStatus.RecordingStatusNotValid,
                                $"Recording {permutation.Id} is in progress");
                            return response;
                        }
                    }
                }

                var key = GetImmediateRecordingKey(epgId, absoluteStartEpoch, absoluteEndEpoch);
                timeBasedRecording = _repository.GetRecordingByKey(groupId, key);
                var shouldIssueRecord = ShouldIssueRecord(timeBasedRecording);

                syncParmeters.Add("shouldInsertRecording", shouldIssueRecord);

                if (shouldIssueRecord)
                {
                    long recordingId = 0;
                    bool syncedAction = synchronizer.DoAction(syncKey, syncParmeters);

                    object recordingObject;
                    if (syncParmeters.TryGetValue("recording", out recordingObject))
                    {
                        recording = (Recording)recordingObject;
                        recordingId = recording.Id;
                    }
                    // all good
                    else
                    {
                        timeBasedRecording = _repository.GetRecordingByKey(groupId, key);
                        recordingId = timeBasedRecording.Id;
                    }

                    // Schedule a message to check duplicate crid
                    DateTime checkTime = _program.EndDate.AddDays(7);
                    eRecordingTask task = eRecordingTask.CheckRecordingDuplicateCrids;
                    RecordingsManager.EnqueueMessage(groupId, epgId, recordingId,
                        _absoluteStartTime, checkTime,
                        task);
                }
                else
                {
                    //Restore current permutation
                    var protectedUntilDate = DateTime.UtcNow.AddDays(accountSettings.ProtectionPeriod ?? 0);
                    var _status = RecordingInternalStatus.OK;
                    if (Enum.IsDefined(typeof(RecordingInternalStatus), timeBasedRecording.Status))
                    {
                        _status = (RecordingInternalStatus)Enum.Parse(typeof(RecordingInternalStatus), timeBasedRecording.Status);
                    }
                        
                    var _hhRecording = new HouseholdRecording(
                        userId,
                        domainId,
                        epgId,
                        key,
                        _status.ToString(),
                        RecordingType.Single.ToString(),
                        DateTime.UtcNow,
                        DateTime.UtcNow,
                        DateUtils.DateTimeToUtcUnixTimestampSeconds(protectedUntilDate),
                        epgChannelId,
                        true
                    );
                    recording = RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, _program,
                        _hhRecording);
                }

                //Overcome exceeded quota
                CheckAndClearHouseholdQuota(groupId, domainId, recording, isQuotaOverage.QuotaOverage,
                    userId.ToString(), epgId);

                if (!recording.Status.IsOkStatusCode())
                {
                    response.Status.Set(recording.Status);
                    return response;
                }

                var hhRecording = SaveToHousehold(groupId, userId, domainId, epgChannelId, epgId, key, recording);
                if (!hhRecording.success)
                {
                    response.SetStatus(eResponseStatus.Error,
                        "Failed saving hh recording");
                    return response;
                }

                // Todo - Matan/Gil - needed? will throw null ref from recording
                if (recording == null && _program != null && timeBasedRecording != null)
                {
                    recording = RecordingsUtils.BuildRecordingFromTBRecording(groupId, timeBasedRecording, _program,
                        hhRecording.rec);
                }

                recording.AbsoluteStartTime = _absoluteStartTime;
                recording.AbsoluteEndTime = _absoluteEndTime;
                recording.EndPadding = absoluteEndOffset;
                recording.Id = hhRecording.rec.Id;
                response.Object = recording;
            }
            catch (Exception ex)
            {
                var msg = $"PaddedRecordingsManager - ImmediateRecord: in record of program {epgId}. error = {ex}";
                log.Error(msg);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private void CheckAndClearHouseholdQuota(int groupId, long domainId, Recording recording, bool quotaOverage,
            string userId, long epgId)
        {
            var recordingDuration = QuotaManager.GetRecordingDurationSeconds(groupId, recording);
            log.Debug($"recordingDuration = {recordingDuration}, quotaOverage = {quotaOverage}");
            if (quotaOverage) // if QuotaOverage then call delete recorded as needed                               
            {
                // handle delete to overage quota
                var deleted = QuotaManager.Instance.HandleDomainAutoDelete(groupId, domainId, recordingDuration);
                if (deleted?.Count > 0)
                {
                    log.Debug($"Quota overage cleaned total of: {deleted.Count} household recordings");
                }
                else
                {
                    log.Error(
                        $"Failed clearing quota, not recordings found, recording: {recording}, epg: {epgId}, domainId: {domainId}, userId: {userId} wasn't added");
                    recording.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExceededQuota,
                        eResponseStatus.ExceededQuota.ToString());
                }
            }
        }

        private (HouseholdRecording rec, bool success) SaveToHousehold(int groupId, long userId, long domainId,
            long epgChannelId, long epgId,
            string key, Recording recording)
        {
            var status = RecordingsUtils.ConvertToDomainRecordingStatus(recording.RecordingStatus).ToString();
            var hhRecording = new HouseholdRecording(userId, domainId, epgId, key,
                status,
                RecordingType.Single.ToString(), DateTime.UtcNow,
                DateTime.UtcNow, recording.ProtectedUntilDate, epgChannelId, true);

            var currentHouseholdRecording =
                _repository.GetHouseholdRecording(groupId, key, domainId);
            
            if (currentHouseholdRecording == null || currentHouseholdRecording.Id == 0) //new hh recording
            {
                if (_repository.AddHouseholdRecording(groupId, hhRecording) == 0)
                {
                    log.Warn("Failed saving HH recording");
                    return (hhRecording, false);
                }
            }
            else // hh recording with the same key
            {
                hhRecording.Id = currentHouseholdRecording.Id;
                if (!_repository.UpdateHouseholdRecording(groupId, hhRecording))
                {
                    log.Warn("Failed updating HH recording");
                    return (hhRecording, false);
                }
            }
            
            LayeredCache.Instance.SetInvalidationKey(
                LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(groupId, domainId));

            return (hhRecording, true);
        }

        public static string GetImmediateRecordingKey(long epgId, long? absoluteStartTime, long? absoluteEndTime)
        {
            return $"{epgId}_{absoluteStartTime ?? 0}_{absoluteEndTime ?? 0}";
        }

        public Status ValidateUpdateRecording(int partnerId, Recording recordingToUpdate, long householdId)
        {
            Status status = new Status(eResponseStatus.OK);
            var accountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(partnerId);
            if (accountSettings.PersonalizedRecordingEnable == true)
            {
                HouseholdRecording householdRecording =
                    _repository.GetHouseholdRecording(partnerId, recordingToUpdate.Id, householdId);

                if (householdRecording == null || householdRecording.Id == 0)
                {
                    return new Status(eResponseStatus.RecordingNotFound,
                        "Recording not found ");
                }

                TimeBasedRecording timeBasedRecording =
                    _repository.GetRecordingByKey(partnerId, householdRecording.RecordingKey);

                if (timeBasedRecording == null || timeBasedRecording.Id == 0)
                {
                    return new Status(eResponseStatus.RecordingNotFound,
                        $"Recording {recordingToUpdate.Id} wasn't found");
                }

                Program program = _repository.GetProgramByEpg(partnerId, timeBasedRecording.EpgId);
                if (program == null || program.Id == 0)
                {
                    return new Status(eResponseStatus.ProgramDoesntExist,
                        $"Program {timeBasedRecording.EpgId} wasn't found");
                }

                if (timeBasedRecording.IsImmediateRecording() && recordingToUpdate.EndPadding.HasValue &&
                    timeBasedRecording.AbsoluteEndTime.Value <= DateTime.UtcNow)
                {
                    status = new Status(eResponseStatus.CanOnlyUpdatePaddingAfterRecordingBeforeRecordingEnd,
                        "Can only update EndPadding recording before recording end");
                }
                else if (recordingToUpdate.StartPadding.HasValue &&
                         program.StartDate.AddMinutes(-1 * timeBasedRecording.PaddingBeforeMins) < DateTime.UtcNow)
                {
                    status = new Status(eResponseStatus.CanOnlyUpdatePaddingBeforeRecordingBeforeRecordingStart,
                        "Can only update StartPadding recording before recording start");
                }
                else if (!timeBasedRecording.IsImmediateRecording() && recordingToUpdate.EndPadding.HasValue &&
                         program.EndDate.AddMinutes(timeBasedRecording.PaddingAfterMins) <= DateTime.UtcNow)
                {
                    status = new Status(eResponseStatus.CanOnlyUpdatePaddingAfterRecordingBeforeRecordingEnd,
                        "Can only update EndPadding recording before scheduled recording end");
                }
            }

            return status;
        }

        public int GetImmediateRecordingTimeSpanSeconds(DateTime? absoluteStartTime, DateTime? absoluteEndTime)
        {
            if (!absoluteStartTime.HasValue || !absoluteEndTime.HasValue)
                return 0;

            var timespan = absoluteEndTime - absoluteStartTime;
            if (timespan?.TotalSeconds <= 0)
                return 0;

            return (int)Math.Ceiling(timespan.Value.TotalSeconds);
        }

        public DateTime GetActualStartDate(TimeShiftedTvPartnerSettings settings, Recording recording)
        {
            if (recording.AbsoluteStartTime.HasValue)
                return recording.AbsoluteStartTime.Value;

            if (recording.StartPadding.HasValue && recording.StartPadding.Value > 0)
                return recording.EpgStartDate.AddMinutes(-1 * recording.StartPadding.Value);

            return recording.EpgStartDate.AddMinutes(-1 * ConditionalAccess.Utils.ConvertSecondsToMinutes((int)(settings.PaddingBeforeProgramStarts ?? 0)));
        }

        public DateTime GetActualEndDate(TimeShiftedTvPartnerSettings settings, Recording recording)
        {
            if (recording.AbsoluteEndTime.HasValue)
                return recording.AbsoluteEndTime.Value;

            if (recording.EndPadding.HasValue && recording.EndPadding.Value > 0)
                return recording.EpgEndDate.AddMinutes(recording.EndPadding.Value);

            return recording.EpgEndDate.AddMinutes(ConditionalAccess.Utils.ConvertSecondsToMinutes((int)(settings.PaddingAfterProgramEnds ?? 0)));
        }

        public bool ScheduleRecordingEvictions()
        {
            List<long> partnersIds = GetAllPartnerIds();
            long expiredTimeWindow = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow.AddHours(1));
            foreach (long partnerId in partnersIds)
            {
                List<TimeBasedRecording> tbRecordings =
                    _repository.GetAndUpdateExpiredRecordings((int)partnerId, expiredTimeWindow);

                foreach (var tbRecording in tbRecordings)
                {
                    SendEvictRecording((int)partnerId, tbRecording.Id, 0,
                        tbRecording.ViewableUntilEpoch);
                }
            }

            return true;
        }

        public bool EvacuateRecording(int partnerId, long oldRecordingDuration, long recordingId)
        {
            bool result = false;
            try
            {
                TimeBasedRecording tbRecording = _repository.GetRecordingById(partnerId, recordingId);
                bool shouldGetDomainRecordings = true;
                if (tbRecording == null || tbRecording.Id == 0)
                {
                    log.DebugFormat("Failed fetching recording with ID: {0} on HandleDomainQuotaByRecording",
                        recordingId);
                    return true;
                }

                Program program = _repository.GetProgramByEpg(partnerId, tbRecording.EpgId);
                Recording recording = RecordingsUtils.BuildRecordingFromTBRecording(partnerId, tbRecording, program);

                // check if expired recording is still valid
                if (recording.RecordingStatus != TstvRecordingStatus.Recorded)
                {
                    log.DebugFormat("Recording has already been deleted/canceled/failed, recordingId:{1}", recordingId);
                    shouldGetDomainRecordings = recording.RecordingStatus != TstvRecordingStatus.Failed;
                    result = true;
                }

                // check to what status we need to update the domain recordings
                DomainRecordingStatus? domainRecordingStatus =
                    ConditionalAccess.Utils.ConvertToDomainRecordingStatus(recording.RecordingStatus);
                if (!domainRecordingStatus.HasValue)
                {
                    log.DebugFormat("Failed ConvertToDomainRecordingStatus for recordingId: {0}, recordingStatus: {1}",
                        recording.Id, recording.RecordingStatus.ToString());
                    shouldGetDomainRecordings = false;
                }

                if (shouldGetDomainRecordings)
                {
                    int status = 1;
                    // Currently canceled can be only due to IngestRecording which Deletes EPG
                    if (domainRecordingStatus.Value != DomainRecordingStatus.OK)
                    {
                        status = 2;
                        domainRecordingStatus = DomainRecordingStatus.DeletedBySystem;
                    }

                    int recordingDurationSeconds = QuotaManager.GetRecordingDurationSeconds(partnerId, recording);
                    int skip = 0;
                    long utcNowEpoch = DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
                    List<HouseholdRecording> modifiedHhRecordings = _repository.UpdateHhRecordingsIdAndProtectDate(
                        partnerId, tbRecording.Key,
                        domainRecordingStatus.Value.ToString(), utcNowEpoch, skip);

                    // set max amount of concurrent tasks
                    int maxDegreeOfParallelism =
                        ApplicationConfiguration.Current.RecordingsMaxDegreeOfParallelism.Value;
                    if (maxDegreeOfParallelism == 0)
                    {
                        maxDegreeOfParallelism = 5;
                    }


                    ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = maxDegreeOfParallelism };
                    while (modifiedHhRecordings.Count > 0)
                    {
                        LogContextData contextData = new LogContextData();
                        Parallel.For(0, modifiedHhRecordings.Count, options, i =>
                        {
                            if (recordingDurationSeconds > 0)
                            {
                                LayeredCache.Instance.SetInvalidationKey(
                                    LayeredCacheKeys.GetDomainRecordingsInvalidationKeys(partnerId,
                                        modifiedHhRecordings[i].HouseholdId));
                            }
                        });

                        skip += modifiedHhRecordings.Count;
                        modifiedHhRecordings = _repository.UpdateHhRecordingsIdAndProtectDate(partnerId,
                            tbRecording.Key,
                            domainRecordingStatus.Value.ToString(), utcNowEpoch, skip);
                    }
                    //
                    // // bulk delete for all domainIds
                    // var isPrivateCopy = Utils.GetTimeShiftedTvPartnerSettings(m_nGroupID).IsPrivateCopyEnabled.Value;
                    //
                    // if (isPrivateCopy)
                    // {
                    //     if (recording.RecordingStatus == TstvRecordingStatus.Failed)
                    //     {
                    //         RecordingsUtils.UpdateIndex(task.GroupId, recording.Id, eAction.Delete);
                    //         RecordingsUtils.UpdateCouchbase(task.GroupId, recording.EpgId, recording.Id, true);
                    //     }
                    //     else if (task.OldRecordingDuration == 0)
                    //     {
                    //         RecordingsManager.Instance.DeleteRecording(task.GroupId, recording, true, false, domainIds.ToList());
                    //     }
                    // }

                    var isUpdate = oldRecordingDuration > 0 && oldRecordingDuration != recordingDurationSeconds;

                    if (!isUpdate)
                    {
                        var hhRecordings =
                            _repository.GetHhRecordingsMinProtectedEpoch(partnerId, tbRecording.Key,
                                tbRecording.ViewableUntilEpoch);
                        var minProtectionEpoch =
                            hhRecordings.Count > 0 ? hhRecordings.Min(x => x.ProtectedUntilEpoch) : 0;
                        // add recording schedule task for next min protected date
                        if (minProtectionEpoch > 0)
                        {
                            SendEvictRecording((int)partnerId, tbRecording.Id, 0,
                                minProtectionEpoch);
                        }
                        else
                        {
                            log.DebugFormat("recordingId: {0} has no domain recording that protect it", recordingId);
                            DeleteRecording(partnerId, recording);
                        }
                    }
                    else
                    {
                        // BEO-11375
                        log.Debug($"recordingId: {recordingId} is being updated");
                    }
                }

                // incase expiredRecording.Id = 0 it means we are handling a FAILED recording and no need to update the table recording_scheduled_tasks 
                // if ( !RecordingsDAL.UpdateExpiredRecordingAfterScheduledTask(task.Id))
                // {
                //     log.ErrorFormat("failed UpdateExpiredRecordingScheduledTask for expiredRecording: {0}", task.ToString());
                // }
                //
                result = true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in HandleDomainQuotaByRecording, ex = {1}, ST = {2}", ex.Message, ex.StackTrace);
            }

            return result;
        }

        public static void SendVerifyRecordingFinalStatus(int partnerId, long recordingId, DateTime etaTime)
        {
            var verifyRecordingFinalStatus = new VerifyRecordingFinalStatus
            {
                PartnerId = partnerId,
                RecordingId = recordingId
            };

            string body = JsonConvert.SerializeObject(verifyRecordingFinalStatus);
            KronosClient.Instance.ScheduledTask(partnerId,
                VerifyRecordingFinalStatus.VerifyRecordingFinalStatusQualifiedName, etaTime.ToUtcUnixTimestampSeconds(),
                body, "recording", 600);
        }

        public static void SendRetryRecording(int partnerId, long recordingId, DateTime etaTime)
        {
            var verifyRecordingFinalStatus = new RetryRecording
            {
                PartnerId = partnerId,
                RecordingId = recordingId
            };

            string body = JsonConvert.SerializeObject(verifyRecordingFinalStatus);
            KronosClient.Instance.ScheduledTask(partnerId, RetryRecording.RetryRecordingQualifiedName,
                etaTime.ToUtcUnixTimestampSeconds(),
                body, "recording", 600);
        }

        public static void SendEvictRecording(int partnerId, long recordingId, long oldRecordingDuration, long etaTime)
        {
            var domainQuotaByRecording = new EvictRecording
            {
                PartnerId = partnerId,
                RecordingId = recordingId,
                OldRecordingDuration = oldRecordingDuration,
            };

            string body = JsonConvert.SerializeObject(domainQuotaByRecording);
            KronosClient.Instance.ScheduledTask(partnerId, EvictRecording.EvictRecordingQualifiedName,
                etaTime,
                body, "recording", 600);
        }

        public bool ValidateRecordingConcurrency(int groupId, long domainId, Recording newRecording)
        {
            var accountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);

            if (!accountSettings.MaxRecordingConcurrency.HasValue || accountSettings.MaxRecordingConcurrency.Value == 0)
                return true;

            var maxRecordingConcurrency = accountSettings.MaxRecordingConcurrency ?? 0;

            //Get all hh recordings
            var statuses = new List<RecordingInternalStatus>
            {
                RecordingInternalStatus.OK, RecordingInternalStatus.Waiting
            }.Select(s => s.ToString()).ToList();

            var allHhRecordings =
                _repository.GetAllHhRecordings(groupId, domainId, statuses,
                    RecordingType.Single.ToString()); //TODO - make selection smaller?
            if (allHhRecordings == null || allHhRecordings.Count < maxRecordingConcurrency)
                return true;

            var recordings = GetRecordingByHouseholdRecording(groupId, allHhRecordings);

            // Based on margin & max reject if needed    
            var parallel = recordings.Select(dic => dic.Value)
                .Where(rec => IsOverlap(groupId, accountSettings, rec, newRecording))
                .ToList();
            return parallel.Count < maxRecordingConcurrency;
        }

        private bool IsOverlap(int groupId, TimeShiftedTvPartnerSettings accountSettings, Recording rec,
            Recording newRecording)
        {
            var oldRecordingStartDate = GetActualStartDate(accountSettings, rec)
                .AddMinutes(accountSettings.MaxConcurrencyMargin ?? 0);
            var oldRecordingEndDate = GetActualEndDate(accountSettings, rec)
                .AddMinutes(-1 * accountSettings.MaxConcurrencyMargin ?? 0);

            if (oldRecordingStartDate >= oldRecordingEndDate)
                return false;
            
            var recordingStartDate = GetActualStartDate(accountSettings, newRecording);
            var recordingEndDate = GetActualEndDate(accountSettings, newRecording);

            bool overlap = oldRecordingStartDate < recordingEndDate && recordingStartDate < oldRecordingEndDate;

            // //https://stackoverflow.com/questions/13513932/algorithm-to-detect-overlapping-periods
            // return !((oldRecordingEndDate < recordingStartDate && oldRecordingStartDate < recordingStartDate) ||
            //          (recordingEndDate < oldRecordingStartDate && recordingStartDate < oldRecordingStartDate));

            return overlap;
        }
    }
}