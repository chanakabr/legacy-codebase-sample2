using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using DAL;
using EpgBL;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using TVinciShared;
using Synchronizer;
using KlogMonitorHelper;
using ApiObjects.QueueObjects;
using ApiObjects.ScheduledTasks;

namespace Recordings
{
    public class  RecordingsManager
    {
        #region Consts

        private const string SCHEDULED_TASKS_ROUTING_KEY = "PROCESS_RECORDING_TASK\\{0}";
        private const string ROUTING_KEY_MODIFIED_RECORDING = "PROCESS_MODIFIED_RECORDING\\{0}";

        private static readonly int MINUTES_ALLOWED_DIFFERENCE = 5;
        private static readonly int MINUTES_RETRY_INTERVAL;
        private static readonly int MAXIMUM_RETRIES_ALLOWED;
        private static readonly bool REMOVE_DUPLICATE_CRIDS;
        private static readonly int CHECK_DUPLICATE_CRID_INTERVAL_SEC;

        #endregion

        #region Data Members

        private Synchronizer.CouchbaseSynchronizer synchronizer = null;

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Singleton

        private RecordingsManager()
        {
            synchronizer = new CouchbaseSynchronizer(1000, 60);
            synchronizer.SynchronizedAct += synchronizer_SynchronizedAct;            
        }

        private static object locker = new object();
        private static RecordingsManager instance;

        public static RecordingsManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new RecordingsManager();
                        }
                    }
                }

                return instance;
            }
        }

        static RecordingsManager()
        {
            REMOVE_DUPLICATE_CRIDS = TVinciShared.WS_Utils.GetTcmBoolValue("RemoveDuplicateCrids");
            CHECK_DUPLICATE_CRID_INTERVAL_SEC = TVinciShared.WS_Utils.GetTcmIntValue("CheckDuplicateCridIntervalSec");
            if (CHECK_DUPLICATE_CRID_INTERVAL_SEC == 0)
            {
                // default is 7 days
                CHECK_DUPLICATE_CRID_INTERVAL_SEC = 604800;
            }
            int retryInterval = TVinciShared.WS_Utils.GetTcmIntValue("CDVRAdapterRetryInterval");
            int maximumRetries = TVinciShared.WS_Utils.GetTcmIntValue("CDVRAdapterMaximumRetriesAllowed");

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

        #endregion

        #region Public Methods

        public Recording Record(int groupId, long programId, long epgChannelID, DateTime startDate, DateTime endDate, string crid)
        {
            Recording recording = null;

            string syncKey = string.Format("RecordingsManager_{0}", programId);
            Dictionary<string, object> syncParmeters = new Dictionary<string, object>();
            syncParmeters.Add("groupId", groupId);
            syncParmeters.Add("programId", programId);
            syncParmeters.Add("crid", crid);
            syncParmeters.Add("epgChannelID", epgChannelID);
            syncParmeters.Add("startDate", startDate);
            syncParmeters.Add("endDate", endDate);            

            try
            {
                Dictionary<long, Recording> recordingsEpgMap = ConditionalAccess.Utils.GetEpgToRecordingsMapByCridAndChannel(groupId, crid, epgChannelID);
                bool insertCheckDuplicateCridTask = true;
                // remember and not forget
                if (recordingsEpgMap.Count == 0)
                {
                    bool syncedAction = synchronizer.DoAction(syncKey, syncParmeters);

                    object recordingObject;
                    if (syncParmeters.TryGetValue("recording", out recordingObject))
                    {
                        recording = (Recording)recordingObject;
                    }
                    // all good
                    else
                    {
                        recording = ConditionalAccess.Utils.GetRecordingByEpgId(groupId, programId);
                    }
                }
                else if (recordingsEpgMap.ContainsKey(programId))
                {
                    recording = recordingsEpgMap[programId];
                }
                else
                {
                    Recording existingRecordingWithMinStartDate = recordingsEpgMap.OrderBy(x => x.Value.EpgStartDate).ToList().First().Value;
                    if (!REMOVE_DUPLICATE_CRIDS || (startDate < existingRecordingWithMinStartDate.EpgStartDate && existingRecordingWithMinStartDate.EpgEndDate > DateTime.UtcNow)
                        || existingRecordingWithMinStartDate.RecordingStatus != TstvRecordingStatus.Recorded)
                    {
                        bool syncedAction = synchronizer.DoAction(syncKey, syncParmeters);

                        object recordingObject;
                        if (syncParmeters.TryGetValue("recording", out recordingObject))
                        {
                            recording = (Recording)recordingObject;
                        }
                        // all good
                        else
                        {
                            recording = ConditionalAccess.Utils.GetRecordingByEpgId(groupId, programId);
                        }
                    }
                    /***** if min recording is already recorded and REMOVE_DUPLICATE_CRIDS is true
                     *     we don't go to the adapter and insert the current recording with the min recording external ID  *****/
                    else if (existingRecordingWithMinStartDate.RecordingStatus == TstvRecordingStatus.Recorded && REMOVE_DUPLICATE_CRIDS)
                    {
                        recording = new Recording(existingRecordingWithMinStartDate) { EpgStartDate = startDate, EpgEndDate = endDate, EpgId = programId, RecordingStatus = TstvRecordingStatus.Scheduled };
                        recording.RecordingStatus = GetTstvRecordingStatus(recording.EpgStartDate, recording.EpgEndDate, recording.RecordingStatus);
                        recording = ConditionalAccess.Utils.InsertRecording(recording, groupId, RecordingInternalStatus.OK);
                        UpdateIndex(groupId, recording.Id, eAction.Update);
                        insertCheckDuplicateCridTask = false;
                    }
                }

                if (insertCheckDuplicateCridTask)
                {
                    // Schedule a message to check duplicate crid
                    DateTime checkTime = endDate.AddSeconds(CHECK_DUPLICATE_CRID_INTERVAL_SEC);
                    eRecordingTask task = eRecordingTask.CheckRecordingDuplicateCrids;
                    EnqueueMessage(groupId, programId, recording.Id, startDate, checkTime, task);
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("RecordingsManager - Record: Error getting recording of program {0}. error = {1}", programId, ex);
                recording = null;
            }

            return recording;
        }

        public Recording RecordRetry(int groupId, long recordingId)
        {
            Recording recording = ConditionalAccess.Utils.GetRecordingById(recordingId);
            try
            {

                // If couldn't find a recording with this ID
                if (recording == null)
                {
                    string message = string.Format("Retry Record - could not find Recording with Id = {0} in group {1}", recordingId, groupId);
                    log.Error(message);
                    recording = new Recording()
                    {
                        Status = new Status((int)eResponseStatus.OK, message)
                    };
                }
                else
                {
                    // if this program is in the past (because it moved, for example)
                    if (recording.EpgStartDate.AddMinutes(1) < DateTime.UtcNow)
                    {
                        string message = string.Format("Retry Record - Recording with Id = {0} in group {1} is already in the past", recordingId, groupId);
                        log.Error(message);
                        recording.Status = new Status((int)eResponseStatus.OK, message);
                    }
                    else
                    {
                        CallAdapterRecord(groupId, recording.ChannelId, recording.EpgStartDate, recording.EpgEndDate, false, recording);

                        // If we got through here withou any exception, we're ok.
                        recording.Status = new Status((int)eResponseStatus.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                if (recording == null)
                {
                    recording = new Recording();
                }

                recording.Status = new Status(
                    (int)eResponseStatus.Error,
                    string.Format("Record retry failure: id = {0}, ex = {1}", recordingId, ex));
            }

            return recording;
        }

        public Status DeleteRecording(int groupId, Recording slimRecording, int adapterId = 0)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (groupId > 0 && slimRecording != null && slimRecording.Id > 0 && slimRecording.EpgId > 0 && !string.IsNullOrEmpty(slimRecording.ExternalRecordingId))
            {
                UpdateIndex(groupId, slimRecording.Id, eAction.Delete);
                UpdateCouchbase(groupId, slimRecording.EpgId, slimRecording.Id, false);
                List<Recording> recordingsWithTheSameExternalId = ConditionalAccess.Utils.GetRecordingsByExternalRecordingId(groupId, slimRecording.ExternalRecordingId);
                // last recording
                if (recordingsWithTheSameExternalId.Count == 1)
                {
                    if (adapterId == 0)
                    {
                        adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
                    }

                    RecordResult adapterResponse = null;
                    var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                    try
                    {
                        //  recording in status scheduled/recording is canceled, otherwise we delete
                        if (slimRecording.EpgEndDate < DateTime.UtcNow)
                        {
                            adapterResponse = adapterController.CancelRecording(groupId, slimRecording.ExternalRecordingId, adapterId);
                        }
                        else
                        {
                            adapterResponse = adapterController.DeleteRecording(groupId, slimRecording.ExternalRecordingId, adapterId);
                        }
                    }
                    catch (KalturaException ex)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
                        return status;
                    }
                    catch (Exception ex)
                    {
                        status = new Status((int)eResponseStatus.Error, "Adapter controller exception: " + ex.Message);
                        return status;
                    }
                }

                status = internalModifyRecording(groupId, slimRecording.Id, slimRecording.EpgEndDate);
            }

            return status;
        }

        public Recording GetRecordingStatus(int groupId, long recordingId)
        {
            Recording currentRecording = ConditionalAccess.Utils.GetRecordingById(recordingId);

            try
            {
                if (currentRecording == null)
                {
                    log.ErrorFormat("GetRecordingStatus: no recording with ID = {0}", recordingId);
                    currentRecording = new Recording()
                    {
                        Status = new Status((int)eResponseStatus.OK,
                            string.Format("No recording with ID = {0} found", recordingId))
                    };
                }
                else
                {
                    // If the program finished already or not: if it didn't finish, then the recording obviously didn't finish...
                    if (currentRecording.EpgEndDate < DateTime.UtcNow)
                    {
                        var timeSpan = DateTime.UtcNow - currentRecording.EpgEndDate;

                        // If this recording is mark as failed, there is no point in trying to get its status
                        if (currentRecording.RecordingStatus == TstvRecordingStatus.Failed)
                        {
                            log.InfoFormat("Rejected GetRecordingStatus request because it is already failed. recordingId = {0}" +
                                "minutesSpan = {1}, allowedDifferenceMinutes = {2}, retryCount = {3}, epg = {4}",
                                recordingId, timeSpan.TotalMinutes, MINUTES_ALLOWED_DIFFERENCE, currentRecording.GetStatusRetries, currentRecording.EpgId);
                        }
                        // Only if this is the first request and the difference is less than 5 minutes we continue
                        // If this is not the first request, we're clear to go even if it is far from being after the program ended
                        else if ((currentRecording.GetStatusRetries == 0 && timeSpan.TotalMinutes < MINUTES_ALLOWED_DIFFERENCE) ||
                            currentRecording.GetStatusRetries > 0)
                        {
                            // Count current try to get status - first and foremost
                            currentRecording.GetStatusRetries++;

                            ConditionalAccess.Utils.UpdateRecording(currentRecording, groupId, 1, 1, null);                                           

                            DateTime nextCheck = DateTime.UtcNow.AddMinutes(MINUTES_RETRY_INTERVAL);

                            int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                            var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                            RecordResult adapterResponse = null;
                            bool shouldRetry = false;

                            try
                            {
                                adapterResponse = adapterController.GetRecordingStatus(groupId, currentRecording.ExternalRecordingId, adapterId);
                            }
                            catch (KalturaException ex)
                            {
                                log.ErrorFormat("GetRecordingStatus: KalturaException when using adapter. ID = {0}, ex = {1}, message = {2}, code = {3}",
                                    recordingId, ex, ex.Message, ex.Data["StatusCode"]);
                                shouldRetry = true;
                            }
                            catch (Exception ex)
                            {
                                log.ErrorFormat("GetRecordingStatus: Exception when using adapter. ID = {0}, ex = {1}", recordingId, ex);
                                shouldRetry = true;
                            }

                            if (adapterResponse == null)
                            {
                                shouldRetry = true;
                            }

                            // Adapter failed for some reason - retry
                            if (shouldRetry)
                            {
                                RetryTaskAfterProgramEnded(groupId, currentRecording, nextCheck, eRecordingTask.GetStatusAfterProgramEnded);
                            }
                            else
                            {
                                //
                                // TODO: Any other validation needed?
                                //

                                // If it was successfull - we mark it as recorded
                                if (adapterResponse.ActionSuccess && adapterResponse.FailReason == 0)
                                {
                                    currentRecording.RecordingStatus = TstvRecordingStatus.Recorded;                                    
                                }
                                else
                                {
                                    currentRecording.RecordingStatus = TstvRecordingStatus.Failed;

                                    currentRecording.Status = CreateFailStatus(adapterResponse);
                                }

                                // Update recording after setting the new status
                                ConditionalAccess.Utils.UpdateRecording(currentRecording, groupId, 1, 1, null);                                
                                UpdateIndex(groupId, recordingId, eAction.Update);
                            }
                        }
                        else
                        {
                            log.InfoFormat("Rejected GetRecordingStatus request because it is too far from the end of the program. recordingId = {0}" +
                                "minutesSpan = {1}, allowedDifferenceMinutes = {2}, retryCount = {3}, epg = {4}",
                                recordingId, timeSpan.TotalMinutes, MINUTES_ALLOWED_DIFFERENCE, currentRecording.GetStatusRetries, currentRecording.EpgId);
                        }
                    }
                }

                // if we didn't have any exception, mark as success, because we don't want the remote task to call CAS again
                // CAS should call GetRecordingStatus only if this method failed completely.
                // Adapter failure doesn't mean an immediate retry!
                if (currentRecording.Status == null)
                {
                    currentRecording.Status = new Status((int)eResponseStatus.OK);
                }
                else
                {
                    currentRecording.Status.Code = (int)eResponseStatus.OK;
                }
            }
            catch (Exception ex)
            {
                currentRecording = new Recording()
                {
                    Status = new Status((int)eResponseStatus.Error,
                        string.Format("Exception {0}", ex))
                };
            }

            return currentRecording;
        }

        public Status UpdateRecording(int groupId, long programId, DateTime startDate, DateTime endDate)
        {
            Status status = new Status();

            Recording recording = ConditionalAccess.Utils.GetRecordingByEpgId(groupId, programId);

            int oldRecordingLength = 0;
            bool shouldUpdateDomainsQuota = false;

            // If there is no recording, nothing to do
            if (recording == null)
            {
                status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            else
            {
                long recordingId = recording.Id;

                // First of all - if EPG was updated, update the recording index, nevermind what was the change
                UpdateIndex(groupId, recordingId, eAction.Update);

                UpdateCouchbase(groupId, programId, recordingId, true);

                // If no change was made to program schedule - do nothing
                if (recording.EpgStartDate == startDate &&
                    recording.EpgEndDate == endDate)
                {
                    // We're OK
                    status = new Status((int)eResponseStatus.OK);
                }
                else
                {
                    oldRecordingLength = (int)(recording.EpgEndDate - recording.EpgStartDate).TotalSeconds;
                    int newRecordingLength = (int)(endDate - startDate).TotalSeconds;

                    if (oldRecordingLength != newRecordingLength)
                    {
                        shouldUpdateDomainsQuota = true;
                    }

                    recording.EpgStartDate = startDate;
                    recording.EpgEndDate = endDate;

                    // deal with CRIDs and stuff
                    List<Recording> recordingsWithTheSameExternalId = ConditionalAccess.Utils.GetRecordingsByExternalRecordingId(groupId, recording.ExternalRecordingId);

                    // get all the recordings with the same ExternalId, if our recording is the only one -> go to the adapter
                    if (recordingsWithTheSameExternalId.Count == 1)
                    {
                        // until proven otherwise - the recording is invalid
                        bool shouldRetry = true;
                        bool shouldMarkAsFailed = true;

                        recording.Status = null;

                        int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                        var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                        // Call Adapter to update recording,

                        // Initialize parameters for adapter controller
                        long startTimeSeconds = ODBCWrapper.Utils.DateTimeToUnixTimestamp(startDate);
                        long durationSeconds = (long)(endDate - startDate).TotalSeconds;

                        RecordResult adapterResponse = null;
                        try
                        {
                            adapterResponse = adapterController.UpdateRecordingSchedule(
                                groupId, recording.ExternalRecordingId, adapterId, startTimeSeconds, durationSeconds);
                        }
                        catch (KalturaException ex)
                        {
                            recording.Status = new Status((int)eResponseStatus.Error,
                                string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
                        }
                        catch (Exception ex)
                        {
                            recording.Status = new Status((int)eResponseStatus.Error, "Adapter controller excpetion: " + ex.Message);
                        }

                        if (adapterResponse == null)
                        {
                            recording.Status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                        }

                        try
                        {
                            RecordingInternalStatus newRecordingInternalStatus = RecordingInternalStatus.Waiting;

                            if (adapterResponse != null)
                            {
                                // Set external recording ID
                                recording.ExternalRecordingId = adapterResponse.RecordingId;
                            }
                            else
                            {
                                shouldRetry = true;
                            }

                            // if adapter failed - retry AND mark as failed
                            // this is because we can't tell the recording is "Fine" when it is far from being fine
                            // its airing time has changed but the provider isn't aware of this... 
                            // so we must inform the users that their recording is not OK
                            if (recording.Status != null && recording.Status.Code != (int)eResponseStatus.OK)
                            {
                                shouldMarkAsFailed = true;
                                shouldRetry = true;
                            }

                            // If we have a resposne AND we didn't set the status to be invalid
                            if (adapterResponse != null && (recording.Status == null || recording.Status.Code == (int)eResponseStatus.OK))
                            {
                                // if provider failed
                                if (!adapterResponse.ActionSuccess || adapterResponse.FailReason != 0)
                                {
                                    shouldRetry = true;
                                    shouldMarkAsFailed = true;
                                }
                                else
                                {
                                    recording.RecordingStatus = TstvRecordingStatus.Scheduled;

                                    recording.RecordingStatus = GetTstvRecordingStatus(recording.EpgStartDate, recording.EpgEndDate, recording.RecordingStatus);

                                    // everything is good
                                    shouldMarkAsFailed = false;
                                    shouldRetry = false;

                                    newRecordingInternalStatus = RecordingInternalStatus.OK;
                                }
                            }

                            if (shouldMarkAsFailed)
                            {
                                recording.RecordingStatus = TstvRecordingStatus.Failed;
                                newRecordingInternalStatus = RecordingInternalStatus.Failed;
                            }

                            // Update the result from the adapter
                            bool updateSuccess = ConditionalAccess.Utils.UpdateRecording(recording, groupId, 1, 1, newRecordingInternalStatus);

                            if (!updateSuccess)
                            {
                                recording.Status = new Status((int)eResponseStatus.Error, "Failed updating recording in database.");
                            }

                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Failed updating recording {0} in database and queue: {1}", recording.Id, ex);
                            recording.Status = new Status((int)eResponseStatus.Error, "Failed inserting/updating recording in database and queue.");
                        }

                        if (shouldRetry)
                        {
                            RetryTaskBeforeProgramStarted(groupId, recording, eRecordingTask.UpdateRecording);
                        }
                    }
                }
            }

            if (shouldUpdateDomainsQuota && status.Code == (int)eResponseStatus.OK)
            {
                //TODO: update domains quota
                GenericCeleryQueue queue = new GenericCeleryQueue();
                RecordingModificationData data = new RecordingModificationData(groupId, 0, recording.Id, 0, oldRecordingLength) { ETA = DateTime.UtcNow };
                bool queueExpiredRecordingResult = queue.Enqueue(data, string.Format(ROUTING_KEY_MODIFIED_RECORDING, groupId));
                if (!queueExpiredRecordingResult)
                {
                    log.ErrorFormat("Failed to queue task in UpdateRecording, recording: {0}", recording.ToString());
                }
            }

            return status;
        }        

        public Recording GetRecordingByProgramId(int groupId, long programId)
        {
            Recording recording = ConditionalAccess.Utils.GetRecordingByEpgId(groupId, programId);
            recording.RecordingStatus = GetTstvRecordingStatus(recording.EpgStartDate, recording.EpgEndDate, recording.RecordingStatus);

            return recording;
        }

        public List<Recording> GetRecordings(int groupId, List<long> recordingIds)
        {
            List<Recording> recordings = ConditionalAccess.Utils.GetRecordings(groupId, recordingIds);

            foreach (var recording in recordings)
            {
                recording.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return recordings;
        }

        public Recording GetRecording(int groupId, long recordingId)
        {
            Recording recording = ConditionalAccess.Utils.GetRecordingById(recordingId);
            if (recording != null)
            {
                recording.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            else
            {
                recording = new Recording()
                {
                    Status = new Status((int)eResponseStatus.RecordingNotFound, eResponseStatus.RecordingNotFound.ToString())
                };
            }
            return recording;
        }

        public List<Recording> GetRecordingsByIdsAndStatuses(int groupId, List<long> recordingIds, List<TstvRecordingStatus> statuses)
        {
            List<Recording> recordings = GetRecordings(groupId, recordingIds);

            var filteredRecording = recordings.Where(recording =>
                statuses.Contains(recording.RecordingStatus)).ToList();

            return filteredRecording;
        }

        public bool CheckRecordingDuplicateCrids(int groupId, long recordingId)
        {
            Recording recording = ConditionalAccess.Utils.GetRecordingById(recordingId);

            if (recording != null && recording.Status != null && recording.Status.Code == (int)eResponseStatus.OK)
            {
                Dictionary<long, Recording> recordingsEpgMap = ConditionalAccess.Utils.GetEpgToRecordingsMapByCridAndChannel(groupId, recording.Crid, recording.ChannelId);
                if (recordingsEpgMap.Count == 0)
                {
                    log.DebugFormat("Failed getting recordingsEpgMap for crid: {0}, channel: {1} from ConditionalAccess.Utils.GetEpgToRecordingsMapByCridAndChannel", recording.Crid, recording.ChannelId);
                    return true;
                }

                Recording existingRecordingWithMinStartDate = recordingsEpgMap.OrderBy(x => x.Value.EpgStartDate).ToList().First().Value;
                // if the min recording is my recordingId and the recordingStatus is RECORDED, we're OK
                if (existingRecordingWithMinStartDate.Id == recordingId && existingRecordingWithMinStartDate.RecordingStatus == TstvRecordingStatus.Recorded)
                {
                    return true;
                }

                // if the min recordingStatis is RECORDED, cancel the current recording on the adapter and update the external recording ID to point to min recording
                if (existingRecordingWithMinStartDate.Id != recordingId && existingRecordingWithMinStartDate.RecordingStatus == TstvRecordingStatus.Recorded)
                {
                    AdapterControllers.CDVR.CdvrAdapterController adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();
                    int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
                    RecordResult adapterResponse = adapterController.CancelRecording(groupId, recording.ExternalRecordingId, adapterId);

                    if (adapterResponse == null)
                    {
                        log.ErrorFormat("Failed CancelRecording, ExternalRecordingId: {0}, Crid: {1}", recording.ExternalRecordingId, recording.Crid);
                        return true;                        
                    }
                    else if (adapterResponse.FailReason != 0)
                    {
                        log.ErrorFormat("Failed CancelRecording, ExternalRecordingId: {0}, Crid: {1}, adapterFailReason: {2}", recording.ExternalRecordingId, recording.Crid, adapterResponse.FailReason);
                        return true; 
                    }

                    recording.ExternalRecordingId = existingRecordingWithMinStartDate.ExternalRecordingId;
                    // update recording information on DB (just external recording ID changes)
                    if (!ConditionalAccess.Utils.UpdateRecording(recording, groupId, 1, 1, RecordingInternalStatus.OK))
                    {
                        log.ErrorFormat("Failed ConditionalAccess.Utils.UpdateRecording for recording ID: {0}", recordingId);
                        return true; 
                    }
                }
            }            
            else
            {
                log.DebugFormat("Failed getting recording with ID: {0} from ConditionalAccess.Utils.GetRecordingById", recordingId);
                return true;
            }

            return true;
        }

        public static TstvRecordingStatus GetTstvRecordingStatus(DateTime epgStartDate, DateTime epgEndDate, TstvRecordingStatus recordingStatus)
        {
            TstvRecordingStatus response = recordingStatus;
            if (recordingStatus == TstvRecordingStatus.Scheduled)
            {
                // If program already finished, we say it is recorded
                if (epgEndDate < DateTime.UtcNow)
                {
                    response = TstvRecordingStatus.Recorded;
                }
                // If program already started but didn't finish, we say it is recording
                else if (epgStartDate < DateTime.UtcNow)
                {
                    response = TstvRecordingStatus.Recording;
                }
            }

            return response;
        }

        public static void EnqueueMessage(int groupId, long programId, long recordingId, DateTime epgStartDate, DateTime etaTime, eRecordingTask task)
        {
            var queue = new GenericCeleryQueue();
            var message = new RecordingTaskData(groupId, task, epgStartDate, etaTime, programId, recordingId);
            queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, groupId));
        }

        internal void RecoverRecordings(int groupId)
        {
            // Get both waiting and failed recordings
            List<int> statuses = new List<int>()
                {
                    (int)RecordingInternalStatus.Failed, (int)RecordingInternalStatus.Waiting
                };

            List<Recording> recordings = ConditionalAccess.Utils.GetAllRecordingsByStatuses(groupId, statuses);

            foreach (var recording in recordings)
            {
                // If the provider failed, we'll start retrying as usual
                if (recording.EpgStartDate > DateTime.UtcNow)
                {
                    EnqueueMessage(groupId, recording.EpgId, recording.Id, recording.EpgStartDate, DateTime.UtcNow, eRecordingTask.Record);
                }

                DateTime getStatusTime = recording.EpgEndDate.AddMinutes(1);

                // Anyway we should always check the status after the program finishes
                EnqueueMessage(groupId, recording.EpgId, recording.Id, recording.EpgStartDate, getStatusTime, eRecordingTask.GetStatusAfterProgramEnded);
            }
        }

        #endregion

        #region Event Methods

        /// <summary>
        /// Actually performs the "Record"
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private bool synchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool success = true;

            int groupId = (int)parameters["groupId"];
            long programId = (long)parameters["programId"];
            string crid = (string)parameters["crid"];
            long epgChannelID = (long)parameters["epgChannelID"];
            DateTime startDate = (DateTime)parameters["startDate"];
            DateTime endDate = (DateTime)parameters["endDate"];            

            Recording recording = ConditionalAccess.Utils.GetRecordingByEpgId(groupId, programId);

            bool issueRecord = false;

            // If there is no recording for this program - create one. This is the first, hurray!
            if (recording == null)
            {
                issueRecord = true;
                recording = new Recording();
                recording.EpgId = programId;
                recording.Crid = crid;
                recording.EpgStartDate = startDate;
                recording.EpgEndDate = endDate;
                recording.ChannelId = epgChannelID;
                recording.RecordingStatus = TstvRecordingStatus.Scheduled;

                // Insert recording information to database
                recording = ConditionalAccess.Utils.InsertRecording(recording, groupId, RecordingInternalStatus.Waiting);
            }
            else if (recording.RecordingStatus == TstvRecordingStatus.Canceled)
            {
                issueRecord = true;
            }

            // If it is a new recording or a canceled recording - we call adapter
            if (issueRecord)
            {
                // Schedule a message to check status 1 minute after recording of program is supposed to be over

                DateTime checkTime = endDate.AddMinutes(1);
                eRecordingTask task = eRecordingTask.GetStatusAfterProgramEnded;

                EnqueueMessage(groupId, programId, recording.Id, startDate, checkTime, task);

                bool isCanceled = recording.RecordingStatus == TstvRecordingStatus.Canceled;

                recording.Status = null;

                // Update Couchbase that the EPG is recorded
                #region Update CB

                UpdateCouchbase(groupId, programId, recording.Id, true);

                #endregion

                // After we know that schedule was succesful,
                // we index data so it is available on search
                if (recording.RecordingStatus == TstvRecordingStatus.OK ||
                    recording.RecordingStatus == TstvRecordingStatus.Recorded ||
                    recording.RecordingStatus == TstvRecordingStatus.Recording ||
                    recording.RecordingStatus == TstvRecordingStatus.Scheduled)
                {
                    UpdateIndex(groupId, recording.Id, eAction.Update);
                }

                // We're OK
                recording.Status = new Status((int)eResponseStatus.OK);
                Recording copyRecording = new Recording(recording);
                ContextData cd = new ContextData();

                // Async - call adapter. Main flow is done                
                System.Threading.Tasks.Task async = Task.Factory.StartNew((taskRecording) =>
                {
                    cd.Load();
                    CallAdapterRecord(groupId, epgChannelID, startDate, endDate, isCanceled, (Recording)taskRecording);
                },
                copyRecording);
            }

            parameters["recording"] = recording;

            return success;
        }

        #endregion

        #region Private Methods

        private static void UpdateIndex(int groupId, long recordingId, eAction action)
        {
            using (ConditionalAccess.WS_Catalog.IserviceClient catalog = new ConditionalAccess.WS_Catalog.IserviceClient())
            {
                catalog.Endpoint.Address = new System.ServiceModel.EndpointAddress(WS_Utils.GetTcmConfigValue("WS_Catalog"));

                long[] objectIds = new long[]{
                                    recordingId
                                };
                catalog.UpdateRecordingsIndex(objectIds, groupId, action);
            }
        }

        private static void UpdateCouchbase(int groupId, long programId, long recordingId, bool isRecorded)
        {
            RecordingCB recording = RecordingsDAL.GetRecordingByProgramId_CB(programId);

            if (isRecorded)
            {
                if (recording == null)
                {
                    TvinciEpgBL epgBLTvinci = new TvinciEpgBL(groupId);

                    EpgCB epg = epgBLTvinci.GetEpgCB((ulong)programId);

                    if (epg != null)
                    {
                        epgBLTvinci.UpdateEpg(epg);

                        recording = new RecordingCB(epg)
                        {
                            RecordingId = (ulong)recordingId
                        };
                    }
                }

                RecordingsDAL.UpdateRecording_CB(recording);
            }
            else if (!isRecorded)
            {
                RecordingsDAL.DeleteRecording_CB(recording);
            }
        }

        private static RecordingCB GetRecordingCB(int groupId, long programId, long recordingId)
        {
            RecordingCB recording = RecordingsDAL.GetRecordingByProgramId_CB(programId);

            if (recording == null)
            {
                TvinciEpgBL epgBLTvinci = new TvinciEpgBL(groupId);

                EpgCB epg = epgBLTvinci.GetEpgCB((ulong)programId);

                if (epg != null)
                {
                    recording = new RecordingCB(epg)
                    {
                        RecordingId = (ulong)recordingId
                    };
                }
            }

            return recording;
        }

        private static void RetryTaskAfterProgramEnded(int groupId, Recording currentRecording, DateTime nextCheck, eRecordingTask recordingTask)
        {
            // Retry in a few minutes if we still didn't exceed retries count
            if (currentRecording.GetStatusRetries <= MAXIMUM_RETRIES_ALLOWED)
            {
                log.DebugFormat("Try to enqueue retry task: groupId {0}, recordingId {1}, nextCheck {2}, recordingTask {3}, retries {4}",
                    groupId, currentRecording.Id, nextCheck.ToString(), recordingTask.ToString(), currentRecording.GetStatusRetries);

                EnqueueMessage(groupId, currentRecording.EpgId, currentRecording.Id, currentRecording.EpgStartDate, nextCheck, recordingTask);
            }
            else
            {
                log.DebugFormat("Exceeded allowed retried count, trying to mark as failed: groupId {0}, recordingId {1}, nextCheck {2}, recordingTask {3}, retries {4}",
                    groupId, currentRecording.Id, nextCheck.ToString(), recordingTask.ToString(), currentRecording.GetStatusRetries);

                // Otherwise, we tried too much! Mark this recording as failed. Sorry mates!
                currentRecording.RecordingStatus = TstvRecordingStatus.Failed;

                // Update recording after updating the status
                ConditionalAccess.Utils.UpdateRecording(currentRecording, groupId, 1, 1, RecordingInternalStatus.Failed);

                // Update all domains that have this recording
                GenericCeleryQueue queue = new GenericCeleryQueue();
                DateTime utcNow = DateTime.UtcNow;
                ApiObjects.QueueObjects.RecordingModificationData data = new ApiObjects.QueueObjects.RecordingModificationData(groupId, 0, currentRecording.Id, 0) { ETA = utcNow };
                bool queueExpiredRecordingResult = queue.Enqueue(data, string.Format(ROUTING_KEY_MODIFIED_RECORDING, groupId));
                if (!queueExpiredRecordingResult)
                {
                    log.ErrorFormat("Failed to queue ExpiredRecording task for RetryTaskAfterProgramEnded when recording FAILED, recordingId: {0}, groupId: {1}", currentRecording.Id, groupId);
                }
            }
        }

        private static void RetryTaskBeforeProgramStarted(int groupId, Recording recording, eRecordingTask task)
        {
            var span = recording.EpgStartDate - DateTime.UtcNow;

            DateTime nextCheck;

            // if there is more than 1 day left, try tomorrow
            if (span.TotalDays > 1)
            {
                log.DebugFormat("Retry task before program started: Recording id = {0} will retry tomorrow, because start date is {1}",
                    recording.Id, recording.EpgStartDate);

                nextCheck = DateTime.UtcNow.AddDays(1);
            }
            else if (span.TotalHours > 1)
            {
                log.DebugFormat("Retry task before program started: Recording id = {0} will retry in half the time (in {1} hours), because start date is {2}",
                    recording.Id, (span.TotalHours / 2), recording.EpgStartDate);

                // if there is less than 1 day, get as HALF as close to the start of the program.
                // e.g. if we are 4 hours away from program, check in 2 hours. If we are 140 minutes away, try in 70 minutes.
                nextCheck = DateTime.UtcNow.AddSeconds(span.TotalSeconds / 2);
            }
            else
            {
                log.DebugFormat("Retry task before program started: Recording id = {0} will retry when program starts, at {1}",
                    recording.Id, recording.EpgStartDate);

                // if we are less than an hour away from the program, try when the program starts (half a minute before it starts)
                nextCheck = recording.EpgStartDate.AddSeconds(-30);
            }

            // continue checking until the program started. 
            if (DateTime.UtcNow < recording.EpgStartDate &&
                DateTime.UtcNow < nextCheck)
            {
                log.DebugFormat("Retry task before program started: program didn't start yet, we will enqueue a message now for recording {0}",
                    recording.Id);
                EnqueueMessage(groupId, recording.EpgId, recording.Id, recording.EpgStartDate, nextCheck, task);
            }
            else
            // If it is still not ok - mark as failed
            {
                log.DebugFormat("Retry task before program started: program started already, we will mark recording {0} as failed.", recording.Id);
                recording.RecordingStatus = TstvRecordingStatus.Failed;

                // Update recording after updating the status
                ConditionalAccess.Utils.UpdateRecording(recording, groupId, 1, 1, RecordingInternalStatus.Failed);

                // Update all domains that have this recording                
                GenericCeleryQueue queue = new GenericCeleryQueue();
                DateTime utcNow = DateTime.UtcNow;
                ApiObjects.QueueObjects.RecordingModificationData data = new ApiObjects.QueueObjects.RecordingModificationData(groupId, 0, recording.Id, 0) { ETA = utcNow };
                bool queueExpiredRecordingResult = queue.Enqueue(data, string.Format(ROUTING_KEY_MODIFIED_RECORDING, groupId));
                if (!queueExpiredRecordingResult)
                {
                    log.ErrorFormat("Failed to queue ExpiredRecording task for RetryTaskAfterProgramEnded when recording FAILED, recordingId: {0}, groupId: {1}", recording.Id, groupId);
                }
            }
        }

        private static Status CreateFailStatus(RecordResult adapterResponse)
        {
            string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}, fail reason = {3}",
                adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage, adapterResponse.FailReason);
            Status failStatus = new Status((int)eResponseStatus.Error, message);
            return failStatus;
        }

        private static void CallAdapterRecord(int groupId, long epgChannelID, DateTime startDate, DateTime endDate, bool isCanceled, Recording currentRecording)
        {
            log.DebugFormat("Call adapter record for recording {0}", currentRecording.Id);
            bool shouldRetry = true;            
            currentRecording.Status = null;
            int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);
            var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();            

            // Initialize parameters for adapter controller
            long startTimeSeconds = ODBCWrapper.Utils.DateTimeToUnixTimestamp(startDate);
            long durationSeconds = (long)(endDate - startDate).TotalSeconds;
            string externalChannelId = CatalogDAL.GetEPGChannelCDVRId(groupId, epgChannelID);

            RecordResult adapterResponse = null;
            try
            {
                adapterResponse = adapterController.Record(groupId, startTimeSeconds, durationSeconds, externalChannelId, adapterId);
            }
            catch (KalturaException ex)
            {
                currentRecording.Status = new Status((int)eResponseStatus.Error,
                    string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
            }
            catch (Exception ex)
            {
                currentRecording.Status = new Status((int)eResponseStatus.Error, "Adapter controller excpetion: " + ex.Message);
            }

            if (adapterResponse == null)
            {
                currentRecording.Status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
            }

            try
            {
                RecordingInternalStatus newRecordingInternalStatus = RecordingInternalStatus.Waiting;

                if (adapterResponse != null)
                {
                    // Set external recording ID
                    currentRecording.ExternalRecordingId = adapterResponse.RecordingId;
                }
                else
                {
                    shouldRetry = true;
                }

                // if adapter failed - retry, don't mark as failed
                if (currentRecording.Status != null && currentRecording.Status.Code != (int)eResponseStatus.OK)
                {                    
                    shouldRetry = true;
                }

                // If we have a resposne AND we didn't set the status to be invalid
                if (adapterResponse != null && (currentRecording.Status == null || currentRecording.Status.Code == (int)eResponseStatus.OK))
                {
                    // if provider failed
                    if (!adapterResponse.ActionSuccess || adapterResponse.FailReason != 0)
                    {
                        shouldRetry = true;                        
                    }
                    else
                    {
                        currentRecording.RecordingStatus = TstvRecordingStatus.Scheduled;
                        currentRecording.RecordingStatus = RecordingsManager.GetTstvRecordingStatus(currentRecording.EpgStartDate, currentRecording.EpgEndDate, currentRecording.RecordingStatus);
                        currentRecording.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                        // Insert the new links of the recordings
                        if (adapterResponse.Links != null && adapterResponse.Links.Count > 0)
                        {
                            RecordingsDAL.InsertRecordingLinks(adapterResponse.Links, groupId, currentRecording.Id);
                        }

                        // everything is good                        
                        shouldRetry = false;

                        newRecordingInternalStatus = RecordingInternalStatus.OK;
                    }
                }

                // Update the result from the adapter
                bool updateSuccess = ConditionalAccess.Utils.UpdateRecording(currentRecording, groupId, 1, 1, newRecordingInternalStatus);

                if (!updateSuccess)
                {
                    currentRecording.Status = new Status((int)eResponseStatus.Error, "Failed updating recording in database.");
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed inserting/updating recording {0} in database and queue: {1}", currentRecording.Id, ex);
                currentRecording.Status = new Status((int)eResponseStatus.Error, "Failed inserting/updating recording in database and queue.");
            }

            if (shouldRetry)
            {
                log.DebugFormat("Call adapter record for recording {0} will retry", currentRecording.Id);
                RetryTaskBeforeProgramStarted(groupId, currentRecording, eRecordingTask.Record);
            }
        }

        private static Status internalModifyRecording(int groupId, long recordingId, DateTime epgEndDate)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // Update all domains that have this recording
            GenericCeleryQueue queue = new GenericCeleryQueue();
            DateTime utcNow = DateTime.UtcNow;
            ApiObjects.QueueObjects.RecordingModificationData data = new ApiObjects.QueueObjects.RecordingModificationData(groupId, 0, recordingId, 0) { ETA = utcNow };
            bool queueExpiredRecordingResult = queue.Enqueue(data, string.Format(ROUTING_KEY_MODIFIED_RECORDING, groupId));
            if (!queueExpiredRecordingResult)
            {
                log.ErrorFormat("Failed to queue ExpiredRecording task for CancelOrDeleteRecording, recordingId: {0}, groupId: {1}", recordingId, groupId);
                return status;
            }

            try
            {
                // Update recording information in to database
                bool updateResult = false;

                if (epgEndDate < DateTime.UtcNow)
                {                    
                    updateResult = RecordingsDAL.CancelRecording(recordingId);
                }
                else
                {
                    updateResult = RecordingsDAL.DeleteRecording(recordingId);
                }

                // We're OK
                if (updateResult)
                {
                    status = new Status((int)eResponseStatus.OK);
                }
                else
                {
                    return new Status((int)eResponseStatus.Error, "Failed CancelRecording or DeleteRecording for on DB");
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error on internalExpireRecording, recordingID: {0}", recordingId), ex);
                status = new Status((int)eResponseStatus.Error, "Exception on CancelRecording or DeleteRecording on DB");
            }

            return status;
        }

        #endregion

    }
}