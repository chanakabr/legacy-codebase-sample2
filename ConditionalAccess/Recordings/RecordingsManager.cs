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

namespace Recordings
{
    public class RecordingsManager
    {
        #region Consts

        private const string SCHEDULED_TASKS_ROUTING_KEY = "PROCESS_RECORDING_TASK\\{0}";

        private static readonly int MINUTES_ALLOWED_DIFFERENCE = 5;
        private static readonly int MINUTES_RETRY_INTERVAL;
        private static readonly int MAXIMUM_RETRIES_ALLOWED;

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

        public Recording Record(int groupId, long programId, string epgChannelID, DateTime startDate, DateTime endDate, string siteGuid, long domainId)
        {
            Recording recording = null;

            string syncKey = string.Format("RecordingsManager_{0}", programId);
            Dictionary<string, object> syncParmeters = new Dictionary<string, object>();
            syncParmeters.Add("groupId", groupId);
            syncParmeters.Add("programId", programId);
            syncParmeters.Add("epgChannelID", epgChannelID);
            syncParmeters.Add("startDate", startDate);
            syncParmeters.Add("endDate", endDate);

            recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);

            // remember and not forget
            if (recording == null)
            {
                bool syncedAction = synchronizer.DoAction(syncKey, syncParmeters);

                object recordingObject;
                if (syncParmeters.TryGetValue("recording", out recordingObject))
                {
                    recording = (Recording)recordingObject;
                }
                else
                {
                    recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);
                }
            }

            try
            {
                if (recording != null)
                {
                    recording.Status = new Status((int)eResponseStatus.OK);
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
            Recording recording = ConditionalAccessDAL.GetRecordingByRecordingId(recordingId);
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

        public Status CancelRecording(int groupId, long programId)
        {
            Status status = new Status();
            Recording recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);

            // If there is no recording for this program - create one. This is the first, hurray!
            if (recording != null)
            {
                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                // Call Adapter to cancel recording schedule

                RecordResult adapterResponse = null;
                try
                {
                    adapterResponse = adapterController.CancelRecording(groupId, recording.ExternalRecordingId, adapterId);
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

                if (recording.Status != null)
                {
                    return recording.Status;
                }

                if (adapterResponse == null)
                {
                    status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                }
                //
                // TODO: Validate adapter response
                //
                else if (adapterResponse.FailReason != 0)
                {
                    status = CreateFailStatus(adapterResponse);
                }
                else
                {
                    try
                    {
                        recording.RecordingStatus = TstvRecordingStatus.Canceled;

                        // Update recording information in to database
                        bool updateResult = ConditionalAccessDAL.UpdateRecording(recording, groupId, 1, 1, null);

                        if (!updateResult)
                        {
                            return new Status((int)eResponseStatus.Error, "Failed update recording");
                        }

                        UpdateCouchbaseIsRecorded(groupId, recording.EpgId, false);

                        // We're OK
                        status = new Status((int)eResponseStatus.OK);
                    }
                    catch (Exception ex)
                    {
                        status = new Status((int)eResponseStatus.Error, "Failed inserting recording to database and queue.");
                    }
                }
            }
            return status;
        }

        public Status DeleteRecording(int groupId, Recording slimRecording)
        {
            Status status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (groupId > 0 && slimRecording != null && slimRecording.Id > 0 && slimRecording.EpgId > 0 && !string.IsNullOrEmpty(slimRecording.ExternalRecordingId))
            {
                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                // Call Adapter to delete recording

                RecordResult adapterResponse = null;
                try
                {
                    adapterResponse = adapterController.DeleteRecording(groupId, slimRecording.ExternalRecordingId, adapterId);
                }
                catch (KalturaException ex)
                {
                    status = new Status((int)eResponseStatus.Error, string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
                    return status;
                }
                catch (Exception ex)
                {
                    status = new Status((int)eResponseStatus.Error, "Adapter controller excpetion: " + ex.Message);
                    return status;
                }

                if (adapterResponse == null)
                {
                    status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                }
                //
                // TODO: Validate adapter response
                //
                else if (adapterResponse.FailReason != 0)
                {
                    status = CreateFailStatus(adapterResponse);
                }
                else
                {
                    try
                    {
                       
                        // Update recording information in to database
                        bool deleteResult = ConditionalAccessDAL.DeleteRecording(slimRecording.Id);

                        if (!deleteResult)
                        {
                            return new Status((int)eResponseStatus.Error, "Failed update recording");
                        }

                        UpdateCouchbaseIsRecorded(groupId, slimRecording.EpgId, false);

                        // We're OK
                        status = new Status((int)eResponseStatus.OK);
                    }
                    catch (Exception ex)
                    {
                        status = new Status((int)eResponseStatus.Error, "Failed inserting recording to database and queue.");
                    }
                }
            }
            return status;
        }

        public Recording GetRecordingStatus(int groupId, long recordingId)
        {
            Recording currentRecording = ConditionalAccessDAL.GetRecordingByRecordingId(recordingId);

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
                    if (currentRecording.EpgEndDate < DateTime.Now)
                    {
                        var timeSpan = DateTime.UtcNow - currentRecording.EpgEndDate;

                        // If this recording is mark as failed, there is no point in tring to get its status
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

                            ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1, null);

                            //UpdateRecording(groupId, currentRecording.EpgId, currentRecording.EpgStartDate, currentRecording.EpgEndDate);

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
                                ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1, null);

                                UpdateIndex(groupId, recordingId);
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

            Recording recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);

            // If there is no recording - error?
            if (recording == null)
            {
                status = new Status((int)eResponseStatus.Error, string.Format("No recording for program {0}", programId));
            }
            else
            {
                long recordingId = recording.Id;

                // First of all - if EPG was updated, update the recording index, nevermind what was the change
                UpdateIndex(groupId, recordingId);

                // If no change was made to program schedule - do nothing
                if (recording.EpgStartDate == startDate &&
                    recording.EpgEndDate == endDate)
                {
                    // We're OK
                    status = new Status((int)eResponseStatus.OK);
                }
                else
                {
                    recording.EpgStartDate = startDate;
                    recording.EpgEndDate = endDate;

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
                        bool updateSuccess = ConditionalAccessDAL.UpdateRecording(recording, groupId, 1, 1, newRecordingInternalStatus);

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

                // OLD code:
                /*
                //    // Update recording data
                //    recording.EpgStartDate = startDate;
                //    recording.EpgEndDate = endDate;

                //    int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                //    var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                //    // Call Adapter to update recording schedule

                //    // Initialize parameters for adapter controller
                //    long startTimeSeconds = ODBCWrapper.Utils.DateTimeToUnixTimestamp(startDate);
                //    long durationSeconds = (long)(endDate - startDate).TotalSeconds;

                //    RecordResult adapterResponse = null;

                //    try
                //    {
                //        adapterResponse = adapterController.UpdateRecordingSchedule(
                //            groupId, recording.ExternalRecordingId, adapterId, startTimeSeconds, durationSeconds);
                //    }
                //    catch (KalturaException ex)
                //    {
                //        recording.Status = new Status((int)eResponseStatus.Error,
                //            string.Format("Code: {0} Message: {1}", (int)ex.Data["StatusCode"], ex.Message));
                //    }
                //    catch (Exception ex)
                //    {
                //        recording.Status = new Status((int)eResponseStatus.Error, "Adapter controller excpetion: " + ex.Message);
                //    }

                //    if (recording.Status != null)
                //    {
                //        return recording.Status;
                //    }

                //    if (adapterResponse == null)
                //    {
                //        status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                //    }
                //    //
                //    // TODO: Validate adapter response
                //    //
                //    else if (adapterResponse.FailReason != 0)
                //    {
                //        status = CreateFailStatus(adapterResponse);
                //    }
                //    else
                //    {
                //        try
                //        {
                //            // Insert recording information to database
                //            bool updateResult = ConditionalAccessDAL.UpdateRecording(recording, groupId, 1, 1, null);

                //            if (!updateResult)
                //            {
                //                return new Status((int)eResponseStatus.Error, "Failed update recording");
                //            }

                //            // Schedule a message tocheck status 1 minute after recording of program is supposed to be over

                //            DateTime checkTime = endDate.AddMinutes(1);
                //            eRecordingTask task = eRecordingTask.GetStatusAfterProgramEnded;

                //            EnqueueMessage(groupId, programId, recordingId, checkTime, task);

                //            // We're OK
                //            status = new Status((int)eResponseStatus.OK);
                //        }
                //        catch (Exception ex)
                //        {
                //            status = new Status((int)eResponseStatus.Error, "Failed inserting recording to database and queue.");
                //        }
                //    }
                //}
                 */
            }

            return status;
        }

        public Recording GetRecordingByProgramId(long programId)
        {
            Recording recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);
            recording.RecordingStatus = GetTstvRecordingStatus(recording.EpgStartDate, recording.EpgEndDate, recording.RecordingStatus);

            return recording;
        }

        public List<Recording> GetRecordings(int groupId, List<long> recordingIds)
        {
            List<Recording> recordings = ConditionalAccessDAL.GetRecordings(groupId, recordingIds);

            foreach (var recording in recordings)
            {
                recording.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return recordings;
        }

        public Recording GetRecording(int groupId, long recordingId)
        {
            Recording recording = ConditionalAccessDAL.GetRecordingByRecordingId(recordingId);
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

        internal void RecoverRecordings(int groupId)
        {
            // Get both waiting and failed recordings
            List<int> statuses = new List<int>()
                {
                    (int)RecordingInternalStatus.Failed, (int)RecordingInternalStatus.Waiting
                };

            List<Recording> recordings = ConditionalAccessDAL.GetAllRecordingsByStatuses(groupId, statuses);

            foreach (var recording in recordings)
            {
                // If the provider failed, we'll start retrying as usual
                if (recording.EpgStartDate > DateTime.UtcNow)
                {
                    EnqueueMessage(groupId, recording.EpgId, recording.Id, DateTime.UtcNow, eRecordingTask.Record);
                }

                DateTime getStatusTime = recording.EpgEndDate.AddMinutes(1);

                // Anyway we should always check the status after the program finishes
                EnqueueMessage(groupId, recording.EpgId, recording.Id, getStatusTime, eRecordingTask.GetStatusAfterProgramEnded);
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
            string epgChannelID = (string)parameters["epgChannelID"];
            DateTime startDate = (DateTime)parameters["startDate"];
            DateTime endDate = (DateTime)parameters["endDate"];

            Recording recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);

            bool issueRecord = false;

            // If there is no recording for this program - create one. This is the first, hurray!
            if (recording == null)
            {
                issueRecord = true;
                recording = new Recording();
                recording.EpgId = programId;
                recording.EpgStartDate = startDate;
                recording.EpgEndDate = endDate;
                recording.ChannelId = epgChannelID;
                recording.RecordingStatus = TstvRecordingStatus.Scheduled;

                // Insert recording information to database
                recording = ConditionalAccessDAL.InsertRecording(recording, groupId, RecordingInternalStatus.Waiting);
            }
            else if (recording.RecordingStatus == TstvRecordingStatus.Canceled)
            {
                issueRecord = true;
            }

            // If it is a new recording or a canceled recording - we call adapter
            if (issueRecord)
            {
                // Schedule a message tocheck status 1 minute after recording of program is supposed to be over

                DateTime checkTime = endDate.AddMinutes(1);
                eRecordingTask task = eRecordingTask.GetStatusAfterProgramEnded;

                EnqueueMessage(groupId, programId, recording.Id, checkTime, task);

                bool isCanceled = recording.RecordingStatus == TstvRecordingStatus.Canceled;

                recording.Status = null;

                // Update Couchbase that the EPG is recorded
                #region Update CB

                UpdateCouchbaseIsRecorded(groupId, programId, true);

                #endregion

                // After we know that schedule was succesful,
                // we index data so it is available on search
                if (recording.RecordingStatus == TstvRecordingStatus.OK ||
                    recording.RecordingStatus == TstvRecordingStatus.Recorded ||
                    recording.RecordingStatus == TstvRecordingStatus.Recording ||
                    recording.RecordingStatus == TstvRecordingStatus.Scheduled)
                {
                    UpdateIndex(groupId, recording.Id);
                }

                // We're OK
                recording.Status = new Status((int)eResponseStatus.OK);

                ContextData cd = new ContextData();

                // Async - call adapter. Main flow is done
                System.Threading.Tasks.Task async = Task.Factory.StartNew((taskRecording) =>
                {
                    cd.Load();

                    Recording copyRecording = (Recording)taskRecording;
                    Recording currentRecording = new Recording()
                    {
                        ChannelId = copyRecording.ChannelId,
                        EpgEndDate = copyRecording.EpgEndDate,
                        EpgId = copyRecording.EpgId,
                        EpgStartDate = copyRecording.EpgStartDate,
                        ExternalRecordingId = copyRecording.ExternalRecordingId,
                        GetStatusRetries = copyRecording.GetStatusRetries,
                        Id = copyRecording.Id,
                        RecordingStatus = copyRecording.RecordingStatus,
                        Status = copyRecording.Status,
                        Type = copyRecording.Type
                    };

                    CallAdapterRecord(groupId, epgChannelID, startDate, endDate, isCanceled, currentRecording);
                },
                recording);
            }

            parameters["recording"] = recording;

            return success;
        }

        #endregion

        #region Private Methods

        private static void UpdateIndex(int groupId, long recordingId)
        {
            using (ConditionalAccess.WS_Catalog.IserviceClient catalog = new ConditionalAccess.WS_Catalog.IserviceClient())
            {
                catalog.Endpoint.Address = new System.ServiceModel.EndpointAddress(WS_Utils.GetTcmConfigValue("WS_Catalog"));

                long[] objectIds = new long[]{
                                    recordingId
                                };
                catalog.UpdateRecordingsIndex(objectIds, groupId, eAction.Update);
            }
        }

        private static void UpdateCouchbaseIsRecorded(int groupId, long programId, bool isRecorded)
        {
            TvinciEpgBL epgBLTvinci = new TvinciEpgBL(groupId);

            EpgCB epg = epgBLTvinci.GetEpgCB((ulong)programId);

            epg.IsRecorded = Convert.ToInt32(isRecorded);

            epgBLTvinci.UpdateEpg(epg);
        }

        private static void EnqueueMessage(int groupId, long programId, long recordingId, DateTime checkTime, eRecordingTask task)
        {
            var queue = new GenericCeleryQueue();
            var message = new RecordingTaskData(groupId, task,
                checkTime,
                programId,
                recordingId);

            queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, groupId));
        }

        private static void RetryTaskAfterProgramEnded(int groupId, Recording currentRecording, DateTime nextCheck, eRecordingTask recordingTask)
        {
            // Retry in a few minutes if we still didn't exceed retries count
            if (currentRecording.GetStatusRetries <= MAXIMUM_RETRIES_ALLOWED)
            {
                log.DebugFormat("Try to enqueue retry task: groupId {0}, recordingId {1}, nextCheck {2}, recordingTask {3}, retries {4}",
                    groupId, currentRecording.Id, nextCheck.ToString(), recordingTask.ToString(), currentRecording.GetStatusRetries);

                EnqueueMessage(groupId, currentRecording.EpgId, currentRecording.Id, nextCheck, recordingTask);
            }
            else
            {
                log.DebugFormat("Exceeded allowed retried count, trying to mark as failed: groupId {0}, recordingId {1}, nextCheck {2}, recordingTask {3}, retries {4}",
                    groupId, currentRecording.Id, nextCheck.ToString(), recordingTask.ToString(), currentRecording.GetStatusRetries);

                // Otherwise, we tried too much! Mark this recording as failed. Sorry mates!
                currentRecording.RecordingStatus = TstvRecordingStatus.Failed;

                // Update recording after updating the status
                ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1, RecordingInternalStatus.Failed);
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
                EnqueueMessage(groupId, recording.EpgId, recording.Id, nextCheck, task);
            }
            else
            // If it is still not ok - mark as failed
            {
                log.DebugFormat("Retry task before program started: program started already, we will mark recording {0} as failed.", recording.Id);

                recording.RecordingStatus = TstvRecordingStatus.Failed;
                ConditionalAccessDAL.UpdateRecording(recording, groupId, 1, 1, RecordingInternalStatus.Failed);
            }
        }

        private static Status CreateFailStatus(RecordResult adapterResponse)
        {
            string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}, fail reason = {3}",
                adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage, adapterResponse.FailReason);
            Status failStatus = new Status((int)eResponseStatus.Error, message);
            return failStatus;
        }

        private static void CallAdapterRecord(int groupId, string epgChannelID, DateTime startDate, DateTime endDate, bool isCanceled, Recording currentRecording)
        {
            log.DebugFormat("Call adapter record for recording {0}", currentRecording.Id);

            bool shouldRetry = true;
            bool shouldMarkAsFailed = false;

            currentRecording.Status = null;

            int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

            var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

            // Call Adapter to schedule recording,

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
                    shouldMarkAsFailed = false;
                    shouldRetry = true;
                }

                // If we have a resposne AND we didn't set the status to be invalid
                if (adapterResponse != null && (currentRecording.Status == null || currentRecording.Status.Code == (int)eResponseStatus.OK))
                {
                    // if provider failed
                    if (!adapterResponse.ActionSuccess || adapterResponse.FailReason != 0)
                    {
                        shouldRetry = true;
                        shouldMarkAsFailed = true;
                    }
                    else
                    {
                        currentRecording.RecordingStatus = TstvRecordingStatus.Scheduled;
                        currentRecording.RecordingStatus = RecordingsManager.GetTstvRecordingStatus(currentRecording.EpgStartDate, currentRecording.EpgEndDate, currentRecording.RecordingStatus);                        

                        // Insert the new links of the recordings
                        if (adapterResponse.Links != null && adapterResponse.Links.Count > 0)
                        {
                            ConditionalAccessDAL.InsertRecordingLinks(adapterResponse.Links, groupId, currentRecording.Id);
                        }

                        // everything is good
                        shouldMarkAsFailed = false;
                        shouldRetry = false;

                        newRecordingInternalStatus = RecordingInternalStatus.OK;
                    }
                }

                if (shouldMarkAsFailed)
                {
                    currentRecording.RecordingStatus = TstvRecordingStatus.Failed;
                    newRecordingInternalStatus = RecordingInternalStatus.Failed;
                }

                // Update the result from the adapter
                bool updateSuccess = ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1, newRecordingInternalStatus);

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

        #endregion

    }
}