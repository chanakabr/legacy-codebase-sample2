using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public partial class KalturaCredentialsProvider : KalturaOTTObject
    {
        /// <summary>
        /// KalturaCognitoIdentity
        /// </summary>
        [DataMember(Name = "cognitoIdentity")]
        [JsonProperty(PropertyName = "cognitoIdentity")]
        [XmlElement(ElementName = "cognitoIdentity")]
        public KalturaCognitoIdentity CognitoIdentity { get; set; }
    }
}
