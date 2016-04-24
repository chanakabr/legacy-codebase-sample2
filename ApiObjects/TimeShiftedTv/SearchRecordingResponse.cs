using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class SearchRecordingResponse
    {

        public ApiObjects.Response.Status Status { get; set; }

        public List<SearchRecording> SearchRecordings { get; set; }

        public SearchRecordingResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            SearchRecordings = new List<SearchRecording>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Status Code: {0}, Status Message: {1} ", Status.Code, Status.Message));
            sb.Append("SearchRecordings: ");
            foreach (Recording searchRecord in SearchRecordings)
            {
                sb.Append(searchRecord.ToString());
                sb.Append(", ");
            }            

            return sb.ToString();
        }

    }
}
