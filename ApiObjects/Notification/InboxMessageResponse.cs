using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class InboxMessageResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<InboxMessage> InboxMessages { get; set; }

        public int TotalCount { get; set; }

    }
}
