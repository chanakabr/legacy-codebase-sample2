using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordingsResponse
    {

        public ApiObjects.Response.Status Status { get; set; }

        public List<RecordingResponse> Recordings { get; set; }

        public RecordingsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Recordings = new List<RecordingResponse>();
        }

        public RecordingsResponse(ApiObjects.Response.Status status, List<RecordingResponse> recordings)
        {
            this.Status = status;
            this.Recordings = recordings;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append("Recordings: ");
            foreach (RecordingResponse record in Recordings)
            {
                sb.Append(record.ToString());
                sb.Append(", ");
            }            

            return sb.ToString();
        }
    }
}
