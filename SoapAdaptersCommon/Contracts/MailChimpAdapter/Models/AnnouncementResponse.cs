using AdapaterCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MailChimpAdapter.Models
{
    public class AnnouncementResponse
    {
        public AdapterStatus Status { get; set; }

        public string AnnouncementExternalId { get; set; }
    }
}