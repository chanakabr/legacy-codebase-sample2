using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
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
    public partial class KalturaEventNotificationFilter : KalturaCrudFilter<KalturaEventNotificationOrderBy, EventNotificationAction>
    {
        /// <summary>
        /// Indicates which event notification to return by their event notifications Id.
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "objectIdidEqualEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string IdEqual { get; set; }

        /// <summary>
        /// Indicates which objectId to return by their event notifications.
        /// </summary>
        [DataMember(Name = "objectIdEqual")]
        [JsonProperty("objectIdEqual")]
        [XmlElement(ElementName = "objectIdEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [SchemeProperty(MinInteger = 1)]
        public long? ObjectIdEqual { get; set; }

        /// <summary>
        /// Indicates which objectType to return by their event notifications.
        /// </summary>
        [DataMember(Name = "eventObjectTypeEqual")]
        [JsonProperty("eventObjectTypeEqual")]
        [XmlElement(ElementName = "eventObjectTypeEqual", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string EventObjectTypeEqual { get; set; }

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
            if( !string.IsNullOrEmpty(IdEqual) && ObjectIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "idEqual", "objectIdEqual");
            }

            if (ObjectIdEqual.HasValue && string.IsNullOrEmpty(EventObjectTypeEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "eventObjectTypeEqual");
            }
           
            if (ObjectIdEqual <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ObjectIdEqual");
            }
        }

        public KalturaEventNotificationFilter() : base()
        {
        }

        public override GenericListResponse<EventNotificationAction> List(ContextData contextData, CorePager pager)
        {
            var coreFilter = AutoMapper.Mapper.Map<EventNotificationActionFilter>(this);
            return EventNotificationActionManager.Instance.List(contextData, coreFilter);
        }
    }

    public enum KalturaEventNotificationOrderBy
    {
        NONE
    }
}
