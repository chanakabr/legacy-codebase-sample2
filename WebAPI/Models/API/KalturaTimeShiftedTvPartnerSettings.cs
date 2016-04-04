using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public class KalturaTimeShiftedTvPartnerSettings : KalturaOTTObject
    {
        /// <summary>
        /// Is catch-up enabled
        /// </summary>
        [DataMember(Name = "catch_up_enabled")]
        [JsonProperty("catch_up_enabled")]
        [XmlElement(ElementName = "catch_up_enabled", IsNullable = true)]
        public bool? CatchUpEnabled { get; set; }

        /// <summary>
        /// Is c-dvr enabled
        /// </summary>
        [DataMember(Name = "cdvr_enabled")]
        [JsonProperty("cdvr_enabled")]
        [XmlElement(ElementName = "cdvr_enabled", IsNullable = true)]
        public bool? CdvrEnabled { get; set; }

        /// <summary>
        /// Is start-over enabled
        /// </summary>
        [DataMember(Name = "start_over_enabled")]
        [JsonProperty("start_over_enabled")]
        [XmlElement(ElementName = "start_over_enabled", IsNullable = true)]
        public bool? StartOverEnabled { get; set; }

        /// <summary>
        /// Is trick-play
        /// </summary>
        [DataMember(Name = "trick_play_enabled")]
        [JsonProperty("trick_play_enabled")]
        [XmlElement(ElementName = "trick_play_enabled", IsNullable = true)]
        public bool? TrickPlayEnabled { get; set; }

        /// <summary>
        /// Catch-up buffer length
        /// </summary>
        [DataMember(Name = "start_over_enabled")]
        [JsonProperty("start_over_enabled")]
        [XmlElement(ElementName = "start_over_enabled", IsNullable = true)]
        public int CatchUpBufferLength { get; set; }

        /// <summary>
        /// Trick play buffer length
        /// </summary>
        [DataMember(Name = "trick_play_enabled")]
        [JsonProperty("trick_play_enabled")]
        [XmlElement(ElementName = "trick_play_enabled", IsNullable = true)]
        public int TrickPlayBufferLength { get; set; }

    }
}