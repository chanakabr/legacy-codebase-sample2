using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Iot client Configuration
    /// </summary>
    [Serializable]
    public partial class KalturaIotClientConfiguration : KalturaOTTObject
    {
        /// <summary>
        /// announcementTopic
        /// </summary>
        [DataMember(Name = "announcementTopic")]
        [JsonProperty(PropertyName = "announcementTopic")]
        [XmlElement(ElementName = "announcementTopic")]
        public string AnnouncementTopic { get; set; }

        /// <summary>
        /// KalturaCredentialsProvider
        /// </summary>
        [DataMember(Name = "credentialsProvider")]
        [JsonProperty(PropertyName = "credentialsProvider")]
        [XmlElement(ElementName = "credentialsProvider")]
        public KalturaCredentialsProvider CredentialsProvider { get; set; }
        /// <summary>
        /// CognitoUserPool
        /// </summary>
        [DataMember(Name = "cognitoUserPool")]
        [JsonProperty(PropertyName = "cognitoUserPool")]
        [XmlElement(ElementName = "cognitoUserPool")]
        public KalturaCognitoUserPool CognitoUserPool { get; set; }

        /// <summary>
        /// json
        /// </summary>
        [DataMember(Name = "json")]
        [JsonProperty(PropertyName = "json")]
        [XmlElement(ElementName = "json")]
        public string Json { get; set; }

        /// <summary>
        /// topics
        /// </summary>
        [DataMember(Name = "topics")]
        [JsonProperty(PropertyName = "topics")]
        [XmlElement(ElementName = "topics")]
        public string Topics { get; set; }
    }
}
