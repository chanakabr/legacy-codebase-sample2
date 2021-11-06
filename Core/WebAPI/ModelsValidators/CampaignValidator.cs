using System;
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
    }
}