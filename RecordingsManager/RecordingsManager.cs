using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using DAL;
using KLogMonitor;
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

            if (recording == null)
            {
                recording = new Recording();
                recording.EpgID = programId;
                recording.StartDate = startDate;
                recording.EndDate = endDate;
                recording.RecordingStatus = TstvRecordingStatus.Scheduled;

                // TODO: Call Adapter to Record
                string externalRecordingId = string.Empty;

                recording.ExternalRecordingId = externalRecordingId;

                ConditionalAccessDAL.InsertRecording(recording, groupId);
            }

            return recording;
        }

        public Status CancelRecord(int groupId, long programId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            Status status = new Status();

            return status;
        }

        #endregion
    }
}
