using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using DAL;
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
                    string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}",
                        adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage);
                    recording.Status = new Status((int)eResponseStatus.AdapterAppFailure, message);
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

                        // Schedule a message tocheck status 1 minute after recording of program is supposed to be over
                        var queue = new GenericCeleryQueue();
                        var message = new RecordingTaskData(groupId, eRecordingTask.GetStatusAfterProgramEnded,
                            // add 1 minutes here
                            endDate.AddMinutes(1),
                            programId,
                            recording.RecordingID);

                        queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, groupId));

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
                    string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}",
                        adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage);
                    status = new Status((int)eResponseStatus.AdapterAppFailure, message);
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
                            string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}",
                                adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage);
                            currentRecording.Status = new Status((int)eResponseStatus.AdapterAppFailure, message);
                        }

                        currentRecording.RecordingStatus = adapterResponse.RecordingState;

                        ConditionalAccessDAL.UpdateRecording(currentRecording, groupId, 1, 1);

                        // After we know that recording was succesful,
                        // we index data so it is available on search
                        if (currentRecording.RecordingStatus == TstvRecordingStatus.Recorded)
                        {
                            using (ConditionalAccess.WS_Catalog.IserviceClient catalog = new ConditionalAccess.WS_Catalog.IserviceClient())
                            {
                                catalog.Endpoint.Address = new System.ServiceModel.EndpointAddress(WS_Utils.GetTcmConfigValue("WS_Catalog"));

                                long[] objectIds = new long[]{
                                    recordingId
                                };
                                catalog.UpdateRecordingsIndex(objectIds, groupId, eAction.On);
                            }
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

            // If there is no recording for this program - create one. This is the first, hurray!
            if (recording != null)
            {
                int adapterId = ConditionalAccessDAL.GetTimeShiftedTVAdapterId(groupId);

                var adapterController = AdapterControllers.CDVR.CdvrAdapterController.GetInstance();

                // Update recording
                recording.EpgStartDate = startDate;
                recording.EpgEndDate = endDate;

                // Call Adapter to update recording schedule

                // Initialize parameters for adapter controller
                long startTimeSeconds = ODBCWrapper.Utils.DateTimeToUnixTimestamp(startDate);
                long durationSeconds = (long)(endDate - startDate).TotalSeconds;

                RecordResult adapterResponse = null;
                try
                {
                    adapterResponse = adapterController.UpdateRecordingSchedule(groupId, recording.ExternalRecordingId, adapterId, startTimeSeconds, durationSeconds);
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
                    string message = string.Format("Adapter failed for reason: {0}. Provider code = {1}, provider message = {2}",
                        adapterResponse.FailReason, adapterResponse.ProviderStatusCode, adapterResponse.ProviderStatusMessage);
                    status = new Status((int)eResponseStatus.AdapterAppFailure, message);
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
                        var queue = new GenericCeleryQueue();
                        var message = new RecordingTaskData(groupId, eRecordingTask.GetStatusAfterProgramEnded,
                            // add 1 minutes here
                            endDate.AddMinutes(1),
                            programId,
                            recording.RecordingID);

                        queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, groupId));

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

        #endregion

    }
}