using AdapaterCommon.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CDVRAdapter.Models
{
    [DataContract]
    public class CloudRecording
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public long EpgId { get; set; }

        [DataMember]
        public long ChannelId { get; set; }

        [DataMember]
        public int RecordingStatus { get; set; }

        [DataMember]
        public int Type { get; set; }

        [DataMember]
        public long? ViewableUntilDate { get; set; }

        [DataMember]
        public long CreateDate { get; set; }

        [DataMember]
        public long UpdateDate { get; set; }

        [DataMember]
        public bool IsProtected { get; set; }

        [DataMember]
        public KeyValue[] MetaData { get; set; }
    }
}