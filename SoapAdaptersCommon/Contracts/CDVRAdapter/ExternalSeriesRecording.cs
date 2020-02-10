using AdapaterCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDVRAdapter.Models
{
    [DataContract]
    public class CloudSeriesRecording
    {
        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string SeriesId { get; set; }

        [DataMember]
        public int SeasonNumber { get; set; }

        [DataMember]
        public long EpgId { get; set; }

        [DataMember]
        public long EpgChannelId { get; set; }

        [DataMember]
        public long CreateDate { get; set; }

        [DataMember]
        public long UpdateDate { get; set; }

        [DataMember]
        public KeyValue[] MetaData { get; set; }
    }
}