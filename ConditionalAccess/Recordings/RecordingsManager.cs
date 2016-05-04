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

        private const int MINUTES_ALLOWED_DIFFERENCE = 5;
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

            if (recording == null)
            {
                bool syncedAction = synchronizer.DoAction(syncKey, syncParmeters);
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

            CallAdapterRecord(groupId, recording.ChannelId, recording.EpgStartDate, recording.EpgEndDate, false, recording);

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
                        bool updateResult = ConditionalAccessDAL.UpdateRecording(recording, groupId, 1, 1);

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

        public Recording GetRecordingStatus(int groupId, long recordingId)
        {
            Recording currentRecording = ConditionalAccessDAL.GetRecordingByRecordingId(recordingId);

            if (currentRecording != null)
            {
                // If the program finished already or not: if it didn't finish, then the recording obviously didn't finish...
                if (currentRecording.EpgEndDate < DateTime.Now)
                {
                    var timeSpan = DateTime.UtcNow - currentRecording.EpgEndDate;

                    // Only if the difference is less than 5 minutes we continue
                    if (timeSpan.TotalMinutes < MINUTES_ALLOWED_DIFFERENCE)
                    {
                        // Count current try to get status - first and foremost
                        currentRecording.GetStatusRetries++;

                        UpdateRecording(groupId, currentRecording.EpgId, currentRecording.EpgStartDate, currentRecording.EpgEndDate);

                        DateTime nextCheck = DateTime.UtcNow.AddMinutes(MINUTES_RETRY_INTERVAL);

                        int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                        var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                        RecordResult adapterResponse = null;
                        try
                        {
                            adapterResponse = adapterController.GetRecordingStatus(groupId, currentRecording.ExternalRecordingId, adapterId);
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

                        if (currentRecording.Status != null)
                        {
                            return currentRecording;
                        }

                        if (adapterResponse == null)
                        {
                            RetryTask(groupId, currentRecording, nextCheck, eRecordingTask.GetStatusAfterProgramEnded);

                            currentRecording.Status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                        }
                        //
                        // TODO: Validate adapter response
                        //
                        else
                        {
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

                            // Update recording after updating the status
                            ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1);

                            UpdateIndex(groupId, recordingId);
                        }
                    }
                }
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
                    // Update recording data
                    recording.EpgStartDate = startDate;
                    recording.EpgEndDate = endDate;

                    int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                    var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                    // Call Adapter to update recording schedule

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
                            // Insert recording information to database
                            bool updateResult = ConditionalAccessDAL.UpdateRecording(recording, groupId, 1, 1);

                            if (!updateResult)
                            {
                                return new Status((int)eResponseStatus.Error, "Failed update recording");
                            }

                            // Schedule a message tocheck status 1 minute after recording of program is supposed to be over

                            DateTime checkTime = endDate.AddMinutes(1);
                            eRecordingTask task = eRecordingTask.GetStatusAfterProgramEnded;

                            EnqueueMessage(groupId, programId, recordingId, checkTime, task);

                            // We're OK
                            status = new Status((int)eResponseStatus.OK);
                        }
                        catch (Exception ex)
                        {
                            status = new Status((int)eResponseStatus.Error, "Failed inserting recording to database and queue.");
                        }
                    }
                }
            }

            return status;
        }

        public Recording GetRecordingByProgramId(long programId)
        {
            Recording recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);
            SetRecordingStatus(recording);

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

            recording.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            return recording;
        }

        public List<Recording> GetRecordingsByIdsAndStatuses(int groupId, List<long> recordingIds, List<TstvRecordingStatus> statuses)
        {
            List<Recording> recordings = GetRecordings(groupId, recordingIds);

            var filteredRecording = recordings.Where(recording =>
                statuses.Contains(recording.RecordingStatus)).ToList();

            return filteredRecording;
        }

        public static void SetRecordingStatus(Recording recording)
        {
            if (recording.RecordingStatus == TstvRecordingStatus.Scheduled)
            {
                // If program already finished, we say it is recorded
                if (recording.EpgEndDate < DateTime.UtcNow)
                {
                    recording.RecordingStatus = TstvRecordingStatus.Recorded;
                }
                // If program already started but didn't finish, we say it is recording
                else if (recording.EpgStartDate < DateTime.UtcNow)
                {
                    recording.RecordingStatus = TstvRecordingStatus.Recording;
                }
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
                recording = ConditionalAccessDAL.InsertRecording(recording, groupId);
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
                // add 1 minutes here
                checkTime,
                programId,
                recordingId);

            queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, groupId));
        }

        private static void RetryTask(int groupId, Recording currentRecording, DateTime nextCheck, eRecordingTask recordingTask)
        {
            // Retry in a few minutes if we still didn't exceed retries count
            if (currentRecording.GetStatusRetries <= MAXIMUM_RETRIES_ALLOWED)
            {
                EnqueueMessage(groupId, currentRecording.EpgId, currentRecording.Id, nextCheck, recordingTask);
            }
            else
            {
                // Otherwise, we tried too much! Mark this recording as failed. Sorry mates!
                currentRecording.RecordingStatus = TstvRecordingStatus.Failed;

                // Update recording after updating the status
                ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1);
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

            // Count this try
            currentRecording.GetStatusRetries++;
            ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1);

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

                        SetRecordingStatus(currentRecording);

                        // if it isn't a canceled recording, it is a completely new one - INSERT links
                        if (!isCanceled)
                        {
                            if (adapterResponse.Links != null)
                            {
                                ConditionalAccessDAL.InsertRecordingLinks(adapterResponse.Links, groupId, currentRecording.Id);
                            }
                        }

                        // everything is good
                        shouldMarkAsFailed = false;
                        shouldRetry = false;
                    }
                }

                if (!shouldMarkAsFailed)
                {
                    currentRecording.RecordingStatus = TstvRecordingStatus.Failed;
                }

                // Update the result from the adapter
                bool updateSuccess = ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1);

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
                var span = currentRecording.EpgStartDate - DateTime.UtcNow;

                DateTime nextCheck;

                // if there is more than 1 day left, try tomorrow
                if (span.TotalDays > 1)
                {
                    nextCheck = DateTime.UtcNow.AddDays(1);
                }
                else if (span.TotalHours > 1)
                {
                    // if there is less than 1 day, get as HALF as close to the start of the program.
                    // e.g. if we are 4 hours away from program, check in 2 hours. If we are 140 minutes away, try in 70 minutes.
                    nextCheck = DateTime.UtcNow.AddSeconds(span.TotalSeconds / 2);
                }
                else
                {
                    // if we are less than an hour away from the program, try when the program starts
                    nextCheck = currentRecording.EpgStartDate;
                }

                // stop checking after the program started
                if (DateTime.UtcNow < currentRecording.EpgStartDate &&
                    DateTime.UtcNow < nextCheck)
                {
                    EnqueueMessage(groupId, currentRecording.EpgId, currentRecording.Id, nextCheck, eRecordingTask.Record);
                }
            }
        }

        #endregion
    }
}