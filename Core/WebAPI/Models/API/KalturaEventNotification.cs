using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Household Coupon details
    /// </summary>
    [Serializable]
    public partial class KalturaEventNotification : KalturaCrudObject<EventNotificationAction, string>
    {
        /// <summary>
        /// Identifier 
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Object identifier 
        /// </summary>
        [DataMember(Name = "objectId")]
        [JsonProperty("objectId")]
        [XmlElement(ElementName = "objectId")]
        [SchemeProperty(MinInteger = 1)]
        public long ObjectId { get; set; }

        /// <summary>
        /// Event object type 
        /// </summary>
        [DataMember(Name = "eventObjectType")]
        [JsonProperty("eventObjectType")]
        [XmlElement(ElementName = "eventObjectType")]
        public string EventObjectType { get; set; }

        /// <summary>
        /// Message 
        /// </summary>
        [DataMember(Name = "message")]
        [JsonProperty("message")]
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Status
        /// </summary>
        [DataMember(Name = "status")]
        [JsonProperty("status")]
        [XmlElement(ElementName = "status")]
        public KalturaEventNotificationStatus Status { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        [DataMember(Name = "actionType")]
        [JsonProperty("actionType")]
        [XmlElement(ElementName = "actionType")]
        public string ActionType { get; set; }

        /// <summary>
        /// Create date
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate")]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Update date
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate")]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        internal override ICrudHandler<EventNotificationAction, string> Handler
        {
            get
            {
                return EventNotificationActionManager.Instance;
            }
        }      

        public KalturaEventNotification() : base() { }

        internal override void SetId(string id)
        {
            Id = id;            
        }

        internal override GenericResponse<EventNotificationAction> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<EventNotificationAction>(this);
            return EventNotificationActionManager.Instance.Update(contextData, coreObject);
        }
    }

    public partial class KalturaEventNotificationListResponse : KalturaListResponse<KalturaEventNotification>
    {
        public KalturaEventNotificationListResponse() : base() { }
    }

    public enum KalturaEventNotificationStatus
    {
        SENT = 0,
        FAILED = 1,
        SUCCESS = 2,
        FAILED_TO_SEND = 3
    }
}