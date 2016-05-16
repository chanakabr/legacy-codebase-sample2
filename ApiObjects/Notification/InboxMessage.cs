using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class InboxMessage
    {
        public string Id { get; set; }
        public long UserId { get; set; }
        public string Message { get; set; }
        public eMessageCategory Category { get; set; }
        public string Url { get; set; }
        public long CreatedAtSec { get; set; }
        public long UpdatedAtSec { get; set; }
        public eMessageState State { get; set; }
    }
}
