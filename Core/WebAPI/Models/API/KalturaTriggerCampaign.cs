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
        private static readonly HashSet<KalturaRuleConditionType> VALID_TRIGGER_CONDITIONS = new HashSet<KalturaRuleConditionType>()
        {
            KalturaRuleConditionType.OR,
            KalturaRuleConditionType.DEVICE_BRAND,
            KalturaRuleConditionType.DEVICE_FAMILY,
            KalturaRuleConditionType.DEVICE_UDID_DYNAMIC_LIST,
            KalturaRuleConditionType.DEVICE_MODEL,
            KalturaRuleConditionType.DEVICE_MANUFACTURER,
            KalturaRuleConditionType.SEGMENTS
        };

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
        [SchemeProperty(IsNullable = true)]
        public List<KalturaCondition> TriggerConditions { get; set; }

        internal override void ValidateForAdd()
        {
            base.ValidateForAdd();

            if (this.TriggerConditions == null || this.TriggerConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "triggerConditions");
            }

            ValidateConditions();
        }

        internal override GenericResponse<Campaign> Add(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<TriggerCampaign>(this);
            return CampaignManager.Instance.AddTriggerCampaign(contextData, coreObject);
        }

        internal override GenericResponse<Campaign> Update(ContextData contextData)
        {
            var coreObject = AutoMapper.Mapper.Map<TriggerCampaign>(this);
            return CampaignManager.Instance.UpdateTriggerCampaign(contextData, coreObject);
        }

        internal override void ValidateForUpdate()
        {
            base.ValidateForUpdate();

            if (this.TriggerConditions != null)
            {
                ValidateConditions();
            }
        }        

        private void ValidateConditions()
        {
            if (TriggerConditions.Count > 50)
            {
                throw new BadRequestException(BadRequestException.MAX_ARGUMENTS, "triggerConditions", 50);
            }

            foreach (var condition in this.TriggerConditions)
            {
                if (!VALID_TRIGGER_CONDITIONS.Contains(condition.Type))
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "triggerConditions", condition.objectType);
                }

                condition.Validate();
            }
        }
    }

    public enum KalturaApiAction
    {
        ADD = 0
    }

    public enum KalturaApiService
    {
        HOUSEHOLD_DEVICE = 0
    }
}