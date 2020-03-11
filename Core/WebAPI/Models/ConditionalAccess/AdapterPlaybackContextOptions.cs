using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    [Obsolete]
    public partial class AdapterPlaybackContextOptions : KalturaPlaybackContextOptions
    {
        public long AdapterId { get; set; }
        public long UserId { get; set; }
        public long StartTimeSeconds { get; set; }
        public string Udid{ get; set; }
        public string DeviceFamily { get; set; }
        public string DeviceBrandId { get; set; }
        public string IP { get; set; }
        public long TimeStamp { get; set; }
        public long Signature { get; set; }
        public KalturaPlaybackContext PlaybackContext { get; set; }
    }
}