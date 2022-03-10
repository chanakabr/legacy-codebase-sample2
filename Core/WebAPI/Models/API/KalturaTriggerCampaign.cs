using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using ApiLogic.Users.Managers;
using ApiObjects.Response;
using ApiObjects.Base;
using ApiObjects;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaTriggerCampaign : KalturaCampaign
    {
        /// <summary>
        /// service
        /// </summary>
        [DataMember(Name = "service")]
        [JsonProperty("service")]
        [XmlElement(ElementName = "service")]
        public KalturaApiService Service { get; set; }

        /// <summary>
        /// action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public KalturaApiAction Action { get; set; }

        /// <summary>
        /// List of conditions for the trigger (conditions on the object)
        /// </summary>
        [DataMember(Name = "triggerConditions")]
        [JsonProperty("triggerConditions")]
        [XmlElement(ElementName = "triggerConditions")]
        [SchemeProperty(IsNullable = true, RequiresPermission = (int)RequestType.READ)]
        public List<KalturaCondition> TriggerConditions { get; set; }
    }
}