using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordingResponse
    {

        public ApiObjects.Response.Status Status { get; set; }

        public Recording Recording { get; set; }

        public string EpgID { get; set; }

        public RecordingResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Recording = null;
            EpgID = null;
        }

        public RecordingResponse(ApiObjects.Response.Status status, Recording recording, int epgID)
        {
            this.Status = status;
            this.Recording = recording;
            this.EpgID = epgID.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format(", Recording: {0}", Recording.ToString()));

            return sb.ToString();
        }
    }
}
