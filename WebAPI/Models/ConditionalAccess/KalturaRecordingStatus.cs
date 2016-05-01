using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaRecordingStatus
    {
        scheduled = 0,
        recording = 1,
        recorded = 2,
        canceled = 3,
        failed = 4,
        does_not_exists = 5,
        deleted = 6
    }

    /// <summary>
    /// Holder object for KalturaRecordingStatus
    /// </summary>    
    public class KalturaRecordingStatusHolder : KalturaOTTObject
    {
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement("status")]
        public KalturaRecordingStatus status { get; set; }
    }    
}