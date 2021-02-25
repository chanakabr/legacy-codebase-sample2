using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.CanaryDeployment
{
    public partial class KalturaCanaryDeploymentDataOwnerShip : KalturaOTTObject
    {
        /// <summary>
        /// AuthenticationMsOwnerShip
        /// </summary>
        [DataMember(Name = "authenticationMsOwnerShip")]
        [JsonProperty("authenticationMsOwnerShip")]
        [XmlElement(ElementName = "authenticationMsOwnerShip")]
        public KalturaCanaryDeploymentAuthenticationMsOwnerShip AuthenticationMsOwnerShip { get; set; }
    }
}
