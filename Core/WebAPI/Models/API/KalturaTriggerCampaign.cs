using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using ApiLogic.Users.Managers;
using ApiObjects.Response;
using ApiObjects.Base;
using ApiObjects;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Campaign
    /// </summary>
    public partial class KalturaTriggerCampaign : KalturaCampaign
    {
        /// <summary>
        /// List of conditions for the trigger (conditions on the object)
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
        public KalturaApiService Service { get; set; }

        /// <summary>
        /// action
        /// </summary>
        [DataMember(Name = "action")]
        [JsonProperty("action")]
        [XmlElement(ElementName = "action")]
        public KalturaApiAction Action { get; set; }

        internal override void ValidateForAdd()
        {
            base.ValidateForAdd();

            if (this.TriggerConditions == null || this.TriggerConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "triggerConditions");
            }

            foreach (var condition in this.DiscountConditions)
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
            //coreObject.EventNotification = GetEventNotification(contextData, coreObject);
            return CampaignManager.Instance.AddTriggerCampaign(contextData, coreObject);
        }

        internal override GenericResponse<Campaign> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<TriggerCampaign>(this);
            return CampaignManager.Instance.UpdateTriggerCampaign(contextData, coreObject);
        }

        internal GenericResponse<Campaign> Dispatch(ContextData contextData, long id)
        {
            return CampaignManager.Instance.DispatchTriggerCampaign(contextData, id);
        }

        internal override void ValidateForUpdate()
        {
            // TODO SHIR - WHAT NEED TO BE VALIDATE?
            base.ValidateForUpdate();
        }        
    }

    public enum KalturaApiAction
    {
        INSERT = 0,
        UPDATE = 1,
    }

    public enum KalturaApiService
    {
        HOUSEHOLD_DEVICE = 0
    }
}