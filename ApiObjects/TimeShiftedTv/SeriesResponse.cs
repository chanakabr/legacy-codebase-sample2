using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class SeriesResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<SeriesRecording> SeriesRecordings { get; set; }

        public int TotalItems { get; set; }

        public SeriesResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            SeriesRecordings = new List<SeriesRecording>();
            TotalItems = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append(string.Format("TotalItems: {0}, ", TotalItems));
            sb.Append("SeriesRecordings: ");
            foreach (SeriesRecording record in SeriesRecordings)
            {
                sb.Append(record.ToString());
                sb.Append(", ");
            }            

            return sb.ToString();
        }
    }
}
