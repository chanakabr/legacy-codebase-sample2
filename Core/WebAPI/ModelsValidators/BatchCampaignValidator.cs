using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class BatchCampaignValidator
    {
        private static readonly HashSet<KalturaRuleConditionType> VALID_BATCH_CONDITIONS = new HashSet<KalturaRuleConditionType>()
        {
            KalturaRuleConditionType.OR,
            KalturaRuleConditionType.SEGMENTS
        };

        public static void ValidateForAdd(this KalturaBatchCampaign model)
        {
            if (model.PopulationConditions == null || model.PopulationConditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "populationConditions");
            }

            model.ValidateConditions();
        }

        public static void ValidateForUpdate(this KalturaBatchCampaign model)
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
    }
}