using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordingCleanupResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public DateTime? LastSuccessfulCleanUpDate { get; set; }
    }
}
