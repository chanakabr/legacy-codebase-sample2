using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.MediaMarks
{
    [Serializable]
    public class MediaMarkLog
    {
        public MediaMarkLog()
        {
            devices = new List<UserMediaMark>();
        }

        [JsonProperty("lm")]
        public UserMediaMark LastMark
        {
            get;
            set;
        }

        [JsonProperty("devices")]
        public List<UserMediaMark> devices
        {
            get;
            set;
        }
    }
}
