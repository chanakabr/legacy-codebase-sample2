using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.MediaMarks
{
    [Serializable]
    public class UserMediaMark
    {
        [JsonProperty("udid")]
        public string UDID { get; set; }
        [JsonProperty("mid")]
        public int MediaID { get; set; }      
        [JsonProperty("uid")]
        public int UserID { get; set; }
        [JsonProperty("loc")]
        public int Location { get; set; }
        [JsonProperty("ts")]
        public DateTime CreatedAt { get; set; }

        public class UMMDateComparer : IComparer<UserMediaMark>
        {

            public int Compare(UserMediaMark x, UserMediaMark y)
            {
                return x.CreatedAt.CompareTo(y.CreatedAt);
            }
        }
    }
}
