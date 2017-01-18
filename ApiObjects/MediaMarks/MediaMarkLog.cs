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
        [JsonProperty("lm")]
        public UserMediaMark LastMark
        {
            get;
            set;
        }
    }
}
