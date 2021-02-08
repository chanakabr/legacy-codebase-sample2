using ApiLogic.Base;
using ApiObjects;
using WebAPI.Models.General;
using ApiObjects.Response;
using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ApiLogic.Notification;

namespace WebAPI.Models.API
{
    /// <summary>
    /// IOT PROFILE
    /// </summary>
    public partial class KalturaIotProfile : KalturaCrudObject<IotProfile, long>
    {
        /// <summary>
        /// adapterUrl
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty(PropertyName = "adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// kalturaIotProfileAws
        /// </summary>
        [DataMember(Name = "iotProfileAws")]
        [JsonProperty(PropertyName = "iotProfileAws")]
        [XmlElement(ElementName = "iotProfileAws")]
        public KalturaIotProfileAws IotProfileAws { get; set; }


        public KalturaIotProfile()
        {

        }

        internal override ICrudHandler<IotProfile, long> Handler
        {
            get
            {
                return IotProfileManager.Instance;
            }
        }

        internal override void SetId(long id)
        {
        }

        internal override GenericResponse<IotProfile> Add(ContextData contextData)
        {
            return IotProfileManager.Instance.Add(contextData);
        }

        internal override GenericResponse<IotProfile> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<IotProfile>(this);
            return IotProfileManager.Instance.Update(contextData, coreObject);
        }

        internal GenericResponse<IotProfile> Get(ContextData contextData)
        {
            return IotProfileManager.Instance.Get(contextData, contextData.GroupId);
        }

        public override void ValidateForAdd()
        {
        }

        internal override void ValidateForUpdate()
        {
        }

        private void Validate()
        {
        }
    }

    /// <summary>
    /// kalturaIotProfileAws
    /// </summary>
    public partial class KalturaIotProfileAws: KalturaCrudObject<IotProfileAws, long>
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

        internal override ICrudHandler<IotProfileAws, long> Handler => throw new System.NotImplementedException();

        internal override void SetId(long id)
        {
        }
    }
}
