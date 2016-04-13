using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordingsManager
{
    public class RecordingsManager
    {
        public int Record(long programId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            int recordingId = 0;

            return recordingId;
        }

        public Status CancelRecord(long programId, DateTime startDate, DateTime endDate, string siteGuid, int domainId)
        {
            Status status = new Status();

            return status;
        }
    }
}
