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

namespace Recordings
{
    public class RecordingsManager
    {
        #region Consts

        private const string SCHEDULED_TASKS_ROUTING_KEY = "PROCESS_RECORDING_TASK\\{0}";

        #endregion

        #region Static Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #endregion

        #region Singleton

        private RecordingsManager()
        {
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

        #endregion

        #region Public Methods

        public Recording Record(int groupId, long programId, string epgChannelID, DateTime startDate, DateTime endDate, string siteGuid, long domainId)
        {
            Recording recording = null;

            recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);

            // If there is no recording for this program - create one. This is the first, hurray!
            if (recording == null)
            {
                recording = new Recording();
                recording.Status = null;
                recording.EpgID = programId;
                recording.EpgStartDate = startDate;
                recording.EpgEndDate = endDate;
                recording.ChannelId = epgChannelID;

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
                    recording.Status = new Status((int)ex.Data["StatusCode"], ex.Message);
                }
                catch (Exception ex)
                {
                    recording.Status = new Status((int)eResponseStatus.AdapterAppFailure, "Adapter controller excpetion: " + ex.Message);
                }

                if (recording.Status != null)
                {
                    return recording;
                }

                if (adapterResponse == null)
                {
                    recording.Status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                }
                //
                // TODO: Validate adapter response
                //
                else if (adapterResponse.FailReason != 0)
                {
                    recording.Status = CreateFailStatus(adapterResponse);
                }
                else
                {
                    try
                    {
                        recording.ExternalRecordingId = adapterResponse.RecordingId;
                        recording.RecordingStatus = adapterResponse.RecordingState;

                        // Insert recording information to database
                        recording = ConditionalAccessDAL.InsertRecording(recording, groupId);

                        if (adapterResponse.Links != null)
                        {
                            ConditionalAccessDAL.InsertRecordingLinks(adapterResponse.Links, groupId, recording.RecordingID);
                        }

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
                            UpdateIndex(groupId, recording.RecordingID);
                        }

                        // Schedule a message tocheck status 1 minute after recording of program is supposed to be over

                        DateTime checkTime = endDate.AddMinutes(1);
                        eRecordingTask task = eRecordingTask.GetStatusAfterProgramEnded;

                        EnqueueMessage(groupId, programId, recording.RecordingID, checkTime, task);

                        // We're OK
                        recording.Status = new Status((int)eResponseStatus.OK);
                    }
                    catch (Exception ex)
                    {
                        recording.Status = new Status((int)eResponseStatus.Error, "Failed inserting recording to database and queue.");
                    }
                }
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
                    recording.Status = new Status((int)ex.Data["StatusCode"], ex.Message);
                }
                catch (Exception ex)
                {
                    recording.Status = new Status((int)eResponseStatus.AdapterAppFailure, "Adapter controller excpetion: " + ex.Message);
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
                        bool updateResult = ConditionalAccessDAL.UpdateRecording(recording, groupId, 2, 0);

                        if (!updateResult)
                        {
                            return new Status((int)eResponseStatus.Error, "Failed update recording");
                        }

                        UpdateCouchbaseIsRecorded(groupId, recording.EpgID, false);

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
                    if (timeSpan.TotalMinutes < 5)
                    {
                        int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                        var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                        RecordResult adapterResponse = null;
                        try
                        {
                            adapterResponse = adapterController.GetRecordingStatus(groupId, currentRecording.ExternalRecordingId, adapterId);
                        }
                        catch (KalturaException ex)
                        {
                            currentRecording.Status = new Status((int)ex.Data["StatusCode"], ex.Message);
                        }
                        catch (Exception ex)
                        {
                            currentRecording.Status = new Status((int)eResponseStatus.AdapterAppFailure, "Adapter controller excpetion: " + ex.Message);
                        }

                        if (currentRecording.Status != null)
                        {
                            return currentRecording;
                        }

                        if (adapterResponse == null)
                        {
                            currentRecording.Status = new Status((int)eResponseStatus.Error, "Adapter controller returned null response.");
                        }
                        //
                        // TODO: Validate adapter response
                        //
                        else if (adapterResponse.FailReason != 0)
                        {
                            currentRecording.Status = CreateFailStatus(adapterResponse);
                        }

                        currentRecording.RecordingStatus = adapterResponse.RecordingState;

                        ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1);

                        // After we know that recording was succesful,
                        // we index data so it is available on search
                        if (currentRecording.RecordingStatus == TstvRecordingStatus.OK ||
                            currentRecording.RecordingStatus == TstvRecordingStatus.Recorded ||
                            currentRecording.RecordingStatus == TstvRecordingStatus.Recording ||
                            currentRecording.RecordingStatus == TstvRecordingStatus.Scheduled)
                        {
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
                long recordingId = recording.RecordingID;

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
                        recording.Status = new Status((int)ex.Data["StatusCode"], ex.Message);
                    }
                    catch (Exception ex)
                    {
                        recording.Status = new Status((int)eResponseStatus.AdapterAppFailure, "Adapter controller excpetion: " + ex.Message);
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
                       status  = CreateFailStatus(adapterResponse);
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

        private static Status CreateFailStatus(RecordResult adapterResponse)
        {
            string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}, fail reason = {3}",
                adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage, adapterResponse.FailReason);
            Status failStatus = new Status((int)eResponseStatus.CdvrAdapterProviderFail, message);
            return failStatus;
        }

        #endregion
    }
}