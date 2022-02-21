using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// kalturaIotProfileAws
    /// </summary>
    public partial class KalturaIotProfileAws : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// iotEndPoint
        /// </summary>
        [DataMember(Name = "iotEndPoint")]
        [JsonProperty(PropertyName = "iotEndPoint")]
        [XmlElement(ElementName = "iotEndPoint")]
        public string IotEndPoint { get; set; }

        /// <summary>
        /// accessKeyId
        /// </summary>
        [DataMember(Name = "accessKeyId")]
        [JsonProperty(PropertyName = "accessKeyId")]
        [XmlElement(ElementName = "accessKeyId")]
        public string AccessKeyId { get; set; }

        /// <summary>
        /// secretAccessKey
        /// </summary>
        [DataMember(Name = "secretAccessKey")]
        [JsonProperty(PropertyName = "secretAccessKey")]
        [XmlElement(ElementName = "secretAccessKey")]
        public string SecretAccessKey { get; set; }

        /// <summary>
        /// userPoolId
        /// </summary>
        [DataMember(Name = "userPoolId")]
        [JsonProperty(PropertyName = "userPoolId")]
        [XmlElement(ElementName = "userPoolId")]
        public string UserPoolId { get; set; }

        /// <summary>
        /// clientId
        /// </summary>
        [DataMember(Name = "clientId")]
        [JsonProperty(PropertyName = "clientId")]
        [XmlElement(ElementName = "clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// identityPoolId
        /// </summary>
        [DataMember(Name = "identityPoolId")]
        [JsonProperty(PropertyName = "identityPoolId")]
        [XmlElement(ElementName = "identityPoolId")]
        public string IdentityPoolId { get; set; }

        /// <summary>
        /// region
        /// </summary>
        [DataMember(Name = "region")]
        [JsonProperty(PropertyName = "region")]
        [XmlElement(ElementName = "region")]
        public string Region { get; set; }

        /// <summary>
        /// updateDate
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty(PropertyName = "updateDate")]
        [XmlElement(ElementName = "updateDate")]
        public long UpdateDate { get; set; }
    }
}
