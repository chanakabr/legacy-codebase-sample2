using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

        /*VersionAdded  - Joker*/

        [JsonProperty("NpvrID", Required = Required.Default)]
        public string NpvrID { get; set; }

        [JsonProperty("playType", Required = Required.Default)]        
        public string playType { get; set; }

        public class UMMDateComparerDesc : IComparer<UserMediaMark>
        {

            public int Compare(UserMediaMark x, UserMediaMark y)
            {
                return y.CreatedAt.CompareTo(x.CreatedAt);
            }
        }

        public class UMMMediaComparer : IComparer<UserMediaMark>
        {

            public int Compare(UserMediaMark x, UserMediaMark y)
            {
                return x.MediaID.CompareTo(y.MediaID);
            }
        }
    }
        
}

