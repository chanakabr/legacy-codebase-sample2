using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [Serializable]
    public partial class KalturaEventNotificationFilter : KalturaCrudFilter<KalturaEventNotificationOrderBy, EventNotificationAction, string, EventNotificationActionFilter>
    {
        /// <summary>
        /// Indicates which objectId to return by their event notifications.
        /// </summary>
        [DataMember(Name = "objectIdEqual")]
        [JsonProperty("objectIdEqual")]
        [XmlElement(ElementName = "objectIdEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinInteger = 1)]
        public long ObjectIdEqual { get; set; }

        /// <summary>
        /// Indicates which objectType to return by their event notifications.
        /// </summary>
        [DataMember(Name = "objectTypeEqual")]
        [JsonProperty("objectTypeEqual")]
        [XmlElement(ElementName = "objectTypeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string ObjectTypeEqual { get; set; }

        public override ICrudHandler<EventNotificationAction, string, EventNotificationActionFilter> Handler
        {
            get
            {
                return EventNotificationActionManager.Instance;
            }
        }

        private static readonly Type relatedObjectFilterType = typeof(KalturaEventNotificationFilter);

        public override Type RelatedObjectFilterType
        {
            get
            {
                return relatedObjectFilterType;
            }
        }

        public override KalturaEventNotificationOrderBy GetDefaultOrderByValue()
        {
            return KalturaEventNotificationOrderBy.NONE;
        }

        public override void Validate()
        {
            if (string.IsNullOrEmpty(ObjectTypeEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ARGUMENT_CANNOT_BE_EMPTYobjectTypeEqual");
            }
           
            if (ObjectIdEqual <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "objectTypeEqual");
            }
        }

        public KalturaEventNotificationFilter() : base()
        {
        }
    }

    public enum KalturaEventNotificationOrderBy
    {
        NONE
    }
}
