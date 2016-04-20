using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordingIDsResponse
    {

        public ApiObjects.Response.Status Status { get; set; }

        public List<long> RecordingIDs { get; set; }

        public RecordingIDsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.RecordingIDs = new List<long>();
        }
    }
}
