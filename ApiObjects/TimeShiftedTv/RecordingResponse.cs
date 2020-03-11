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

        public List<Recording> Recordings { get; set; }

        public int TotalItems { get; set; }

        public RecordingResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Recordings = new List<Recording>();
            TotalItems = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("TotalItems: {0}, ", TotalItems));
            sb.Append("Recordings: ");
            foreach (Recording record in Recordings)
            {
                sb.Append(record.ToString());
                sb.Append(", ");
            }            

            return sb.ToString();
        }
    }
}
