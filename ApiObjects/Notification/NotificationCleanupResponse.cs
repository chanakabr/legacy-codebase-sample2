using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class NotificationCleanupResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public long LastCleanupDate { get; set; }
    }
}
