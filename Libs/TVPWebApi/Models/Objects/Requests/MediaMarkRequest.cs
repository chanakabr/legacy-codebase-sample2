using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.TVMDataLoader.Protocols.MediaMark;

namespace TVPWebApi.Models
{
    public class MediaMarkRequest : MediaHitRequest
    {
        public action action { get; set; }
    }
}