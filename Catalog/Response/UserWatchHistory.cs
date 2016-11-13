using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Catalog.Response
{
    [DataContract]
    [Serializable]
    public class UserWatchHistory : BaseObject
    {
        [JsonProperty("uid")]
        [DataMember]
        public int UserID
        {
            get;
            set;
        }

        //[JsonProperty("assetId")]
        //public string AssetId { get; set; }

        [JsonProperty("loc")]
        [DataMember]
        public int Location
        {
            get;
            set;
        }

        [JsonProperty("duration")]
        [DataMember]
        public int Duration
        {
            get;
            set;
        }

        [JsonProperty("ts")]
        [DataMember]
        public long LastWatch
        {
            get;
            set;
        }

        [JsonProperty("assetTypeId")]
        [DataMember]
        public int AssetTypeId
        {
            get;
            set;
        }

        //public DateTime AssetUpdatedDate { get; set; }

        [DataMember]
        public bool IsFinishedWatching
        {
            get;
            set;
        }

        [JsonProperty("epgId")]
        [DataMember]
        public long EpgId
        {
            get;
            set;
        }
    }
}
