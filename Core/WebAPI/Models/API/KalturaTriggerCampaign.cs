using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using ApiLogic.Users.Managers;
using ApiObjects.Response;
using ApiObjects.Base;
using ApiObjects;
using System.Linq;
using System;
using WebAPI.Managers.Models;

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
            if (methodParams.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ACTION_NOT_SPECIFIED);
            }

            if (Action != "add" && Action != "update")
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "Action");
            }

            var typeMap = AutoMapper.Mapper.Configuration.GetAllTypeMaps().FirstOrDefault(x => x.SourceType == methodParams.First().Value.Type);
            if (typeMap == null || !typeMap.DestinationType.IsSubclassOf(typeof(CoreObject)))
            {
                throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETERS);
            }

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
            coreObject.EventNotification = GetEventNotification(contextData, coreObject);
            return CampaignManager.Instance.AddTriggerCampaign(contextData, coreObject);
        }

        private string GetEventNotification(ContextData contextData, TriggerCampaign coreObject)
        {
            var _event = new EventNotification()
            {
                PartnerId = contextData.GroupId,
                Actions = new List<NotificationAction> 
                {
                    new EventNotifications.CampaignHandler
                    {
                        Status = 0,
                        SystemName = coreObject.SystemName,
                        FriendlyName = coreObject.Name,
                        CampaignId = coreObject.Id
                    }
                },
            };
            return JsonConvert.SerializeObject(_event);
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