using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// User asset rule filter
    /// </summary>
    public partial class KalturaPlaybackProfileFilter : KalturaFilter<KalturaPlaybackProfileOrderBy>
    {
        /// <summary>
        /// Playback profile to filter by
        /// </summary>
        [DataMember(Name = "playbackProfileEqual")]
        [JsonProperty("playbackProfileEqual")]
        [XmlElement(ElementName = "playbackProfileEqual")]
        public int? PlaybackProfileIdEqual { get; set; }
       
        internal int getPlaybackProfileId()
        {
            return PlaybackProfileIdEqual.HasValue ? (int)PlaybackProfileIdEqual : 0;
        }

        public override KalturaPlaybackProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaPlaybackProfileOrderBy.NAME_ASC;
        }

        internal void Validate()
        {
            
        }
    }

    public enum KalturaPlaybackProfileOrderBy
    {
        NAME_ASC
    }
}