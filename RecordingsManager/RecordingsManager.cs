using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using DAL;
using DalCB;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
                recording = new Recording(programId);
                recording.EpgID = programId;
                recording.StartDate = startDate;
                recording.EndDate = endDate;
                recording.RecordingStatus = TstvRecordingStatus.Scheduled;

                // TODO: Call Adapter to schedule recording,
                // 
                string externalRecordingId = string.Empty;

                recording.ExternalRecordingId = externalRecordingId;

                // Insert recording information to database
                recording = ConditionalAccessDAL.InsertRecording(recording, groupId);

                // Schedule a message tocheck status 5 minutes after recording of program is supposed to be over
                var queue = new GenericCeleryQueue();
                var message = new RecordingTaskData(groupId, eRecordingTask.GetStatusAfterProgramEnded, 
                    // add 5 minutes here
                    endDate.AddMinutes(5),
                    programId,
                    recording.RecordingID);

                queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, groupId));
            }

            return recording;
        }

        public Status CancelRecord(int groupId, long programId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            Status status = new Status();

            return status;
        }

        #endregion

        public Recording CheckFinishedRecordingStatus(int groupId, long recordingId)
        {
            Recording result = null;

            Recording currentRecording = ConditionalAccessDAL.GetRecordingByRecordingId(recordingId);

            if (currentRecording != null)
            {
                // First we get information about the program - if it really finished at this time
                EpgDal_Couchbase dal = new EpgDal_Couchbase(groupId);

                var epg = dal.GetProgram(currentRecording.EpgID.ToString());

                // If the program finished already or not: if it didn't finish, then the recording obviously didn't finish...
                if (epg.EndDate < DateTime.Now)
                {
                    var timeSpan = DateTime.UtcNow - epg.EndDate;

                    // Only if the difference is less than 5 minutes we continue
                    if (timeSpan.TotalMinutes < 5)
                    {
                        // TODO: Call Adapter to check status of recording,
                        // 
                        // TODO: Update recording object according to response from adapter

                        ConditionalAccessDAL.UpdateRecording(currentRecording, groupId);

                        // After we know that recording was succesful,
                        // we index data so it is available on search
                        if (currentRecording.RecordingStatus == TstvRecordingStatus.Recorded)
                        {
                            using (WS_Catalog.IserviceClient catalog = new WS_Catalog.IserviceClient())
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

            return result;
        }
    }
}