using ApiLogic.Base;
using ApiObjects;
using WebAPI.Models.General;
using ApiObjects.Response;
using ApiObjects.Base;
using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Iot client Configuration
    /// </summary>
    [Serializable]
    public partial class KalturaIotClientConfiguration : KalturaOTTObject
    {
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
    }

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

    public partial class KalturaCognitoIdentity : KalturaOTTObject
    {
        /// <summary>
        /// Default
        /// </summary>
        [DataMember(Name = "default")]
        [JsonProperty(PropertyName = "default")]
        [XmlElement(ElementName = "default")]
        public KalturaDefault Default { get; set; }
    }

    public partial class KalturaCognitoUserPool : KalturaOTTObject
    {
        /// <summary>
        /// Default
        /// </summary>
        [DataMember(Name = "default")]
        [JsonProperty(PropertyName = "default")]
        [XmlElement(ElementName = "default")]
        public KalturaDefault Default { get; set; }
    }

    public partial class KalturaDefault : KalturaOTTObject
    {
        /// <summary>
        /// PoolId
        /// </summary>
        [DataMember(Name = "poolId")]
        [JsonProperty(PropertyName = "poolId")]
        [XmlElement(ElementName = "poolId")]
        public string PoolId { get; set; }
        /// <summary>
        /// Region
        /// </summary>
        [DataMember(Name = "region")]
        [JsonProperty(PropertyName = "region")]
        [XmlElement(ElementName = "region")]
        public string Region { get; set; }
        /// <summary>
        /// AppClientId
        /// </summary>
        [DataMember(Name = "appClientId")]
        [JsonProperty(PropertyName = "appClientId")]
        [XmlElement(ElementName = "appClientId")]
        public string AppClientId { get; set; }
    }
}
