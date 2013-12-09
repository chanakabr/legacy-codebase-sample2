using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;

namespace TVPWebApi.Models
{
    public class MediaHitRequest
    {
        public int media_type { get; set; }
        public long file_id { get; set; }
        public int location { get; set; }
    }
}