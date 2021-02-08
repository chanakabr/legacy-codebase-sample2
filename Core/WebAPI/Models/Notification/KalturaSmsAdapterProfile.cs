using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using WebAPI.Models.General;
using ApiLogic.Notification.Managers;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Exceptions;
using WebAPI.Utils;

namespace WebAPI.Models.Notification
{
    /// <summary>
    /// Sms adapter profile
    /// </summary>
    public partial class KalturaSmsAdapterProfile : KalturaCrudObject<SmsAdapterProfile, long>
    {
        /// <summary>
        /// id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// adapter url
        /// </summary>
        [DataMember(Name = "adapterUrl")]
        [JsonProperty("adapterUrl")]
        [XmlElement(ElementName = "adapterUrl")]
        [SchemeProperty(MinLength = 1, MaxLength = 256)]
        public string AdapterUrl { get; set; }

        /// <summary>
        /// Shared Secret
        /// </summary>
        [DataMember(Name = "sharedSecret")]
        [JsonProperty("sharedSecret")]
        [XmlElement(ElementName = "sharedSecret")]
        [SchemeProperty(MinLength = 1, MaxLength = 256)]
        public string SharedSecret { get; set; }

        /// <summary>
        /// SSO Adapter is active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive")]
        public int? IsActive { get; set; }

        /// <summary>
        /// SSO Adapter extra parameters
        /// </summary>
        [DataMember(Name = "settings")]
        [JsonProperty("settings")]
        [XmlElement(ElementName = "settings", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> Settings { get; set; }

        /// <summary>
        /// SSO Adapter external identifier
        /// </summary>
        [DataMember(Name = "externalIdentifier")]
        [JsonProperty("externalIdentifier")]
        [XmlElement(ElementName = "externalIdentifier")]
        public string ExternalIdentifier { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }


        public KalturaSmsAdapterProfile()
        {

        }

        internal override ICrudHandler<SmsAdapterProfile, long> Handler
        {
            get
            {
                return SmsManager.Instance;
            }
        }

        internal override void SetId(long id)
        {
            this.Id = id;
        }

        internal override GenericResponse<SmsAdapterProfile> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<SmsAdapterProfile>(this);
            coreObject.GroupId = contextData.GroupId;
            return SmsManager.Instance.Add(contextData, coreObject);
        }

        internal override GenericResponse<SmsAdapterProfile> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<SmsAdapterProfile>(this);
            coreObject.GroupId = contextData.GroupId;
            return SmsManager.Instance.Update(contextData, coreObject);
        }

        public GenericListResponse<SmsAdapterProfile> List(ContextData contextData)
        {
            var response = new GenericListResponse<SmsAdapterProfile>();
            var groupId = contextData.GroupId;

            try
            {
                // call client
                var _response = SmsManager.Instance.GetSmsAdapters(groupId);

                if (_response.RespStatus.IsOkStatusCode())
                {
                    response.SetStatus(eResponseStatus.OK);
                    response.Objects = _response.SmsAdapters.Select(x => new SmsAdapterProfile
                    {
                        AdapterUrl = x.AdapterUrl,
                        ExternalIdentifier = x.ExternalIdentifier,
                        GroupId = x.GroupId,
                        Id = x.Id,
                        IsActive = x.IsActive == 1,
                        Name = x.Name,
                        Settings = x.Settings,
                        SharedSecret = x.SharedSecret
                    }).ToList();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        public override void ValidateForAdd()
        {
            if (this == null) { throw new ClientException((int)eResponseStatus.NoAdapterToInsert, "No sms adapter to add"); }
            if (string.IsNullOrEmpty(this.AdapterUrl)) { throw new ClientException((int)eResponseStatus.AdapterUrlRequired, "Adapter Url Required"); }
            if (string.IsNullOrEmpty(this.SharedSecret)) { throw new ClientException((int)eResponseStatus.SharedSecretRequired, "Shared Secret Required"); }
        }

        internal override void ValidateForUpdate()
        {
            if (this.Id == 0)
            {
                throw new ClientException((int)eResponseStatus.IdentifierRequired, "Id is missing");
            }
        }
    }
}
