using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class TriggerCampaignValidator
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

        public static void ValidateForAdd(this KalturaTriggerCampaign model)
        {
            if (model.TriggerConditions != null)
            {
                model.ValidateConditions();
            }
        }

        public static void ValidateForUpdate(this KalturaTriggerCampaign model)
        {
            if (model.TriggerConditions != null)
            {
                model.ValidateConditions();
            }
        }

        private static void ValidateConditions(this KalturaTriggerCampaign model)
        {
            if (model.TriggerConditions.Count > 50)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "triggerConditions", 50);
            }

            foreach (var condition in model.TriggerConditions)
            {
                if (!VALID_TRIGGER_CONDITIONS.Contains(condition.Type))
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "triggerConditions", condition.objectType);
                }

                condition.Validate(VALID_TRIGGER_CONDITIONS);
            }
        }
    }
}