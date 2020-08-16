using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using ApiLogic.Users.Managers;
using ApiLogic.Base;
using ApiObjects.Response;
using ApiObjects.Base;
using ApiObjects;

namespace WebAPI.Models.API
{
    // TODO MATAN - ADD MAP BETWEEN KalturaTriggerCampain -> TriggerCampaign AND TriggerCampaign -> KalturaTriggerCampain 
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaTriggerCampain : KalturaCampaign
    {
        // TODO SHIR - FILL ALL
        /// <summary>
        /// List of conditions for the trigger (condions on the object)
        /// </summary>
        [DataMember(Name = "triggerConditions")]
        [JsonProperty("triggerConditions")]
        [XmlElement(ElementName = "triggerConditions")]
        public List<KalturaCondition> TriggerConditions { get; set; }

        /// <summary>
        /// service
        /// </summary>
        [DataMember(Name = "service")]
        [JsonProperty("service")]
        [XmlElement(ElementName = "service")]
        public string Service { get; set; }

        /// <summary>
        /// action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        internal override void ValidateForAdd()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
            base.ValidateForAdd();

            if (string.IsNullOrEmpty(this.Service))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "service");
            }

            if (string.IsNullOrEmpty(this.Action))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "action");
            }

            var methodParams = WebAPI.Reflection.DataModel.getMethodParams(this.Service, this.Action);
            
            if (this.TriggerConditions == null || this.TriggerConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "triggerConditions");
            }

            foreach (var condition in this.CampaignConditions)
            {
                if (condition.Type != KalturaRuleConditionType.OR && condition.Type != KalturaRuleConditionType.TRIGGER)
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "triggerConditions", condition.objectType);
                }

                condition.Validate();
            }
        }

        internal override GenericResponse<Campaign> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<TriggerCampaign>(this);
            return CampaignManager.Instance.AddTriggerCampaign(contextData, coreObject);
        }

        internal override void ValidateForUpdate()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
            base.ValidateForUpdate();
        }

        internal override GenericResponse<Campaign> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<TriggerCampaign>(this);
            return CampaignManager.Instance.UpdateTriggerCampaign(contextData, coreObject);
        }
    }
}