using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Base;
using System;
using ApiLogic.Users.Managers;
using System.Collections.Generic;

namespace WebAPI.Models.API
{
    public partial class KalturaCampaignSearchFilter : KalturaCampaignFilter
    {
        /// <summary>
        /// start Date Greater Than Or Equal
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty("startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual", IsNullable = true)]
        public long? StartDateGreaterThanOrEqual { get; set; }

        /// <summary>
        /// end Date Greater Than Or Equal
        /// </summary>
        [DataMember(Name = "endDateLessThanOrEqual")]
        [JsonProperty("endDateLessThanOrEqual")]
        [XmlElement(ElementName = "endDateLessThanOrEqual", IsNullable = true)]
        public long? EndDateLessThanOrEqual { get; set; }

        /// <summary>
        /// state Equal
        /// </summary>
        [DataMember(Name = "stateEqual")]
        [JsonProperty("stateEqual")]
        [XmlElement(ElementName = "stateEqual", IsNullable = true)]
        public KalturaObjectState? StateEqual { get; set; }

        /// <summary>
        /// has Promotion
        /// </summary>
        [DataMember(Name = "hasPromotion")]
        [JsonProperty("hasPromotion")]
        [XmlElement(ElementName = "hasPromotion", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool? HasPromotion { get; set; }
    }
}