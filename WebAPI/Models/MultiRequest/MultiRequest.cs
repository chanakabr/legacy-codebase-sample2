using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.General;

namespace WebAPI.Models.MultiRequest
{
    public class MultiRequest : KalturaOTTObject
    {
        public string service { get; set; }
        public string action { get; set; }
        public string[] parameters { get; set; }
    }
}