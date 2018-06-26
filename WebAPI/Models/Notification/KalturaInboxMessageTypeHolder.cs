using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// Holder object for inbox message type enum
    /// </summary>    
    public partial class KalturaInboxMessageTypeHolder : KalturaOTTObject
    {
        [DataMember(Name = "type")]
        [JsonProperty("type")]
        [XmlElement("type")]
        public KalturaInboxMessageType type { get; set; }
    }
}