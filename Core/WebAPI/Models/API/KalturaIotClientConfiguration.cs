using WebAPI.Models.General;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Iot client Configuration
    /// </summary>
    [Serializable]
    public partial class KalturaIotClientConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// IdentityPoolId
        /// </summary>
        [DataMember(Name = "identityPoolId")]
        [JsonProperty(PropertyName = "identityPoolId")]
        [XmlElement(ElementName = "identityPoolId")]
        public string IdentityPoolId { get; set; }
        
        /// <summary>
        /// UserPoolId
        /// </summary>
        [DataMember(Name = "userPoolId")]
        [JsonProperty(PropertyName = "userPoolId")]
        [XmlElement(ElementName = "userPoolId")]
        public string UserPoolId { get; set; }
        
        /// <summary>
        /// AwsRegion
        /// </summary>
        [DataMember(Name = "awsRegion")]
        [JsonProperty(PropertyName = "awsRegion")]
        [XmlElement(ElementName = "awsRegion")]
        public string AwsRegion { get; set; }
        
        /// <summary>
        /// appClientId
        /// </summary>
        [DataMember(Name = "appClientId")]
        [JsonProperty(PropertyName = "appClientId")]
        [XmlElement(ElementName = "appClientId")]
        public string AppClientId { get; set; }
        
        /// <summary>
        /// legacyEndPoint
        /// </summary>
        [DataMember(Name = "legacyEndPoint")]
        [JsonProperty(PropertyName = "legacyEndPoint")]
        [XmlElement(ElementName = "legacyEndPoint")]
        public string LegacyEndPoint { get; set; }
        
        /// <summary>
        /// endPoint
        /// </summary>
        [DataMember(Name = "endPoint")]
        [JsonProperty(PropertyName = "endPoint")]
        [XmlElement(ElementName = "endPoint")]
        public string EndPoint { get; set; }
        
        /// <summary>
        /// thingName
        /// </summary>
        [DataMember(Name = "thingName")]
        [JsonProperty(PropertyName = "thingName")]
        [XmlElement(ElementName = "thingName")]
        public string ThingName { get; set; }
        
        /// <summary>
        /// thingArn
        /// </summary>
        [DataMember(Name = "thingArn")]
        [JsonProperty(PropertyName = "thingArn")]
        [XmlElement(ElementName = "thingArn")]
        public string ThingArn { get; set; }
        
        /// <summary>
        /// thingId
        /// </summary>
        [DataMember(Name = "thingId")]
        [JsonProperty(PropertyName = "thingId")]
        [XmlElement(ElementName = "thingId")]
        public string ThingId { get; set; }
        
        /// <summary>
        /// username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty(PropertyName = "username")]
        [XmlElement(ElementName = "username")]
        public string Username { get; set; }
        
        /// <summary>
        /// password
        /// </summary>
        [DataMember(Name = "password")]
        [JsonProperty(PropertyName = "password")]
        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// topics
        /// </summary>
        [DataMember(Name = "topics")]
        [JsonProperty(PropertyName = "topics")]
        [XmlElement(ElementName = "topics")]
        public List<KalturaKeyValue> Topics { get; set; }
        
        /// <summary>
        /// status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty(PropertyName = "status")]
        [XmlElement(ElementName = "status")]
        public string Status { get; set; }
        
        /// <summary>
        /// message
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty(PropertyName = "message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }
    }
}