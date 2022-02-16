using ApiLogic.Base;
using ApiObjects;
using WebAPI.Models.General;
using ApiObjects.Response;
using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.API
{
    /// <summary>
    /// IOT DEVICE
    /// </summary>
    public partial class KalturaIot : KalturaOTTObjectSupportNullable
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember(Name = "udid")]
        [JsonProperty(PropertyName = "udid")]
        [XmlElement(ElementName = "udid")]
        public string Udid { get; set; }

        //Cognito
        /// <summary>
        /// accessKey
        /// </summary>
        [DataMember(Name = "accessKey")]
        [JsonProperty(PropertyName = "accessKey")]
        [XmlElement(ElementName = "accessKey")]
        public string AccessKey { get; set; }
        /// <summary>
        /// accessSecretKey
        /// </summary>
        [DataMember(Name = "accessSecretKey")]
        [JsonProperty(PropertyName = "accessSecretKey")]
        [XmlElement(ElementName = "accessSecretKey")]
        public string AccessSecretKey { get; set; }
        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty(PropertyName = "Username")]
        [XmlElement(ElementName = "Username")]
        public string Username { get; set; }
        /// <summary>
        /// UserPassword
        /// </summary>
        [DataMember(Name = "userPassword")]
        [JsonProperty(PropertyName = "userPassword")]
        [XmlElement(ElementName = "userPassword")]
        public string UserPassword { get; set; }
        /// <summary>
        /// IdentityId
        /// </summary>
        [DataMember(Name = "identityId")]
        [JsonProperty(PropertyName = "identityId")]
        [XmlElement(ElementName = "identityId")]
        public string IdentityId { get; set; }

        //Iot
        /// <summary>
        /// ThingArn
        /// </summary>
        [DataMember(Name = "thingArn")]
        [JsonProperty(PropertyName = "thingArn")]
        [XmlElement(ElementName = "thingArn")]
        public string ThingArn { get; set; }
        /// <summary>
        /// ThingId
        /// </summary>
        [DataMember(Name = "thingId")]
        [JsonProperty(PropertyName = "thingId")]
        [XmlElement(ElementName = "thingId")]
        public string ThingId { get; set; }
        /// <summary>
        /// Principal
        /// </summary>
        [DataMember(Name = "principal")]
        [JsonProperty(PropertyName = "principal")]
        [XmlElement(ElementName = "principal")]
        public string Principal { get; set; }
        /// <summary>
        /// EndPoint
        /// </summary>
        [DataMember(Name = "endPoint")]
        [JsonProperty(PropertyName = "endPoint")]
        [XmlElement(ElementName = "endPoint")]
        public string EndPoint { get; set; }
        /// <summary>
        /// ExtendedEndPoint
        /// </summary>
        [DataMember(Name = "extendedEndPoint")]
        [JsonProperty(PropertyName = "extendedEndPoint")]
        [XmlElement(ElementName = "extendedEndPoint")]
        public string ExtendedEndPoint { get; set; }

        //env
        /// <summary>
        /// IdentityPoolId
        /// </summary>
        [DataMember(Name = "identityPoolId")]
        [JsonProperty(PropertyName = "identityPoolId")]
        [XmlElement(ElementName = "identityPoolId")]
        public string IdentityPoolId { get; set; }
    }
}
