using ApiObjects;
using ApiObjects.Response;
using ApiObjects.TimeShiftedTv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordingsManager
{
    public class RecordingsManager
    {
        public Recording Record(int groupId, long programId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            Recording recording = null;

            recording = DAL.ConditionalAccessDAL.GetRecordingByProgramId(programId);

            return recording;
        }

        public Status CancelRecord(int groupId, long programId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            Status status = new Status();

            return status;
        }
    }
}
