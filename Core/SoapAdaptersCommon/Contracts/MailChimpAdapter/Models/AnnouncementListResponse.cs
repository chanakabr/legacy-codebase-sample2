using AdapaterCommon.Models;
using System.Collections.Generic;

namespace MailChimpAdapter.Models
{
    public class AnnouncementListResponse
    {
        public AdapterStatus Status { get; set; }

        public List<string> AnnouncementExternalId { get; set; }
    }
}