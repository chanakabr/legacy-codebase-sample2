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

        public Recording Record(int groupId, long programId, string externalChannelId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            Recording recording = null;

            recording = ConditionalAccessDAL.GetRecordingByProgramId(programId);

            // If there is no recording for this program - create one. This is the first, hurray!
            if (recording == null)
            {
                recording = new Recording();
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

        public Recording GetRecordingStatus(int groupId, long recordingId)
        {
            Recording result = null;

            Recording currentRecording = ConditionalAccessDAL.GetRecordingByRecordingId(recordingId);

            if (currentRecording != null)
            {
                // TODO: Call Adapter to check status of recording,
                // 
                // TODO: Update recording object according to response from adapter

                ConditionalAccessDAL.UpdateRecording(currentRecording, groupId);
            }

            return result;
        }
    }
}
