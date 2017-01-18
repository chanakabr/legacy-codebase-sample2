using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class UserRecording : Recording
    {
        public string UserId { get; set; }

        public UserRecording()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Id = 0;            
            this.Type = RecordingType.Single;
        }

        public UserRecording(Recording record, string userId)
            : base(record)
        {
            this.UserId = userId;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());
            sb.Append(string.Format("UserId: {0}, ", UserId));

            return sb.ToString();
        }

    }
}
