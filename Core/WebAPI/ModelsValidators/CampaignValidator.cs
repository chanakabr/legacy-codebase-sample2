using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class CampaignValidator
    {
        public static void ValidateForAdd(this KalturaCampaign model)
        {
            model.ValidateDates(false);

            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrWhiteSpace(model.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (string.IsNullOrEmpty(model.SystemName) || string.IsNullOrWhiteSpace(model.SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (string.IsNullOrEmpty(model.Message))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "message");
            }

            if (model.Promotion != null)
            {
                model.Promotion.Validate();
            }

            switch (model)
            {
                case KalturaBatchCampaign c: c.ValidateForAdd(); break;
                case KalturaTriggerCampaign c: c.ValidateForAdd(); break;
                default: throw new NotImplementedException($"ValidateForAdd for {model.objectType} is not implemented");
            }
        }

        public static void ValidateForUpdate(this KalturaCampaign model)
        {
            model.ValidateDates(true);

            if (model.Promotion != null)
            {
                model.Promotion.Validate();
            }

            switch (model)
            {
                case KalturaBatchCampaign c: c.ValidateForUpdate(); break;
                case KalturaTriggerCampaign c: c.ValidateForUpdate(); break;
                default: throw new NotImplementedException($"ValidateForUpdate for {model.objectType} is not implemented");
            }
        }

        private static void ValidateDates(this KalturaCampaign model, bool isUpdate)
        {
            if (model.StartDate == 0 && model.EndDate != 0)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "StartDate", "EndDate");
            }

            if (model.StartDate != 0 && model.EndDate == 0)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "EndDate", "StartDate");
            }

            var now = TVinciShared.DateUtils.GetUtcUnixTimestampNow();
            if (model.EndDate <= now && (!isUpdate || model.EndDate != 0))
            {
                throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "EndDate");
            }

            if (model.StartDate < now && (!isUpdate || model.StartDate != 0))
            {
                throw new BadRequestException(BadRequestException.TIME_ARGUMENT_IN_PAST, "StartDate");
            }

            if (model.EndDate <= model.StartDate && model.StartDate != 0 && model.EndDate != 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "StartDate", "EndDate");
            }
        }

        private static readonly HashSet<KalturaRuleConditionType> VALID_BATCH_CONDITIONS = new HashSet<KalturaRuleConditionType>()
        {
            KalturaRuleConditionType.OR,
            KalturaRuleConditionType.SEGMENTS
        };

        private static void ValidateForAdd(this KalturaBatchCampaign model)
        {
            if (model.PopulationConditions == null || model.PopulationConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "populationConditions");
            }

            model.ValidateConditions();
        }

        private static void ValidateForUpdate(this KalturaBatchCampaign model)
        {
            if (model.PopulationConditions != null)
            {
                model.ValidateConditions();
            }
        }

        private static void ValidateConditions(this KalturaBatchCampaign model)
        {
            if (model.PopulationConditions.Count > 50)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "populationConditions", 50);
            }

            foreach (var condition in model.PopulationConditions)
            {
                if (!VALID_BATCH_CONDITIONS.Contains(condition.Type))
                {
                    throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "populationConditions", condition.objectType);
                }

                condition.Validate(VALID_BATCH_CONDITIONS);
            }
        }

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

        private static void ValidateForAdd(this KalturaTriggerCampaign model)
        {
            if (model.TriggerConditions != null)
            {
                model.ValidateConditions();
            }
        }

        private static void ValidateForUpdate(this KalturaTriggerCampaign model)
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