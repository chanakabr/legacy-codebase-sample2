using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class MediaMarkObject
    {
        public int group_id { get; set; }

        public string site_guid { get; set; }

        public int media_id { get; set; }

        public int location_sec { get; set; }

        public string device_name { get; set; }

        public string device_id { get; set; }

        public MediaMarkObjectStatus status { get; set; }
    }
}

