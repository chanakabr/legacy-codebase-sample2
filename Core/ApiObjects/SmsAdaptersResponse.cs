using ApiObjects.Notification;
using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class SmsAdaptersResponse
    {
        public Status RespStatus { get; set; }
        public IEnumerable<SmsAdapter> SmsAdapters { get; set; }

        public SmsAdaptersResponse()
        {
            RespStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            SmsAdapters = new List<SmsAdapter>();
        }
    }
}
