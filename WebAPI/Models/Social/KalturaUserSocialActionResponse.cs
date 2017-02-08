using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaUserSocialActionResponse : KalturaOTTObject
    {
        /// <summary>
        /// socialAction
        /// </summary>
        [DataMember(Name = "socialAction")]
        [JsonProperty("socialAction")]
        [XmlElement(ElementName = "socialAction", IsNullable = true)]        
        public KalturaSocialAction SocialAction { get; set; }
        
        /// <summary>
        /// List of action permission items
        /// </summary>
        [DataMember(Name = "failStatus")]
        [JsonProperty("failStatus")]
        [XmlArray(ElementName = "failStatus", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaNetworkActionStatus> NetworkStatus { get; set; }
    }

    public class KalturaNetworkActionStatus : KalturaOTTObject
    {
        /// <summary>
        /// Status 
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaSocialStatus Status { get; set; }

        /// <summary>
        /// Social network 
        /// </summary>
        [DataMember(Name = "network")]
        [JsonProperty("network")]
        [XmlElement(ElementName = "network", IsNullable = true)]
        public KalturaSocialNetwork? Network { get; set; }
    }
}