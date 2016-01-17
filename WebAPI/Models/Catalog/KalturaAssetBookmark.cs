using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;
using WebAPI.Models.Users;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// The last position in a media / NPVR / EPG asset which a user watched  
    /// </summary>
    [Serializable]
    public class KalturaAssetBookmark : KalturaOTTObject
    {
        /// <summary>
        ///User object
        /// </summary>
        [DataMember(Name = "user")]
        [JsonProperty("user")]
        [XmlElement(ElementName = "user", IsNullable = true)]        
        public KalturaBaseOTTUser User { get; set; }

        /// <summary>
        ///The position of the user in the specific asset (in seconds)
        /// </summary>
        [DataMember(Name = "position")]
        [JsonProperty("position")]
        [XmlElement(ElementName = "position")]
        public int Position { get; set; }

        /// <summary>
        ///Indicates who is the owner of this position
        /// </summary>
        [DataMember(Name = "position_owner")]
        [JsonProperty("position_owner")]
        [XmlElement(ElementName = "position_owner", IsNullable = true)]
        public KalturaPositionOwner PositionOwner { get; set; }

        /// <summary>
        ///Specifies whether the user's current position exceeded 95% of the duration
        /// </summary>
        [DataMember(Name = "finished_watching")]
        [JsonProperty("finished_watching")]
        [XmlElement(ElementName = "finished_watching")]
        public bool IsFinishedWatching { get; set; }

    }
}

 