using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaEpgChannel : KalturaOTTObject
    {
        /// <summary>
        /// Unique identifier for the epg channel
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }
        /// <summary>
        /// epg channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// epg channel description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// Specifies when was the epg channel was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the epg channel last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Enable cDVR
        /// </summary>
        [DataMember(Name = "enableCdvr")]
        [JsonProperty(PropertyName = "enableCdvr")]
        [XmlElement(ElementName = "enableCdvr")]        
        public bool? EnableCdvr { get; set; }

        /// <summary>
        /// Enable catch-up
        /// </summary>
        [DataMember(Name = "enableCatchUp")]
        [JsonProperty(PropertyName = "enableCatchUp")]
        [XmlElement(ElementName = "enableCatchUp")]        
        public bool? EnableCatchUp { get; set; }

        /// <summary>
        /// Enable start over
        /// </summary>
        [DataMember(Name = "enableStartOver")]
        [JsonProperty(PropertyName = "enableStartOver")]
        [XmlElement(ElementName = "enableStartOver")]        
        public bool? EnableStartOver { get; set; }

        /// <summary>
        /// Catch-up buffer
        /// </summary>
        [DataMember(Name = "catchUpBuffer")]
        [JsonProperty(PropertyName = "catchUpBuffer")]
        [XmlElement(ElementName = "catchUpBuffer")]        
        public long? CatchUpBuffer { get; set; }

        /// <summary>
        /// Trick-play buffer
        /// </summary>
        [DataMember(Name = "trickPlayBuffer")]
        [JsonProperty(PropertyName = "trickPlayBuffer")]
        [XmlElement(ElementName = "trickPlayBuffer")]        
        public long? TrickPlayBuffer { get; set; }

        /// <summary>
        /// Enable Recording playback for non entitled channel
        /// </summary>
        [DataMember(Name = "enableRecordingPlaybackNonEntitledChannel")]
        [JsonProperty(PropertyName = "enableRecordingPlaybackNonEntitledChannel")]
        [XmlElement(ElementName = "enableRecordingPlaybackNonEntitledChannel")]
        [SchemeProperty(ReadOnly = true)]        
        public bool? EnableRecordingPlaybackNonEntitledChannel { get; set; }

        /// <summary>
        /// Enable trick-play
        /// </summary>
        [DataMember(Name = "enableTrickPlay")]
        [JsonProperty(PropertyName = "enableTrickPlay")]
        [XmlElement(ElementName = "enableTrickPlay")]        
        public bool? EnableTrickPlay { get; set; }
    }
}