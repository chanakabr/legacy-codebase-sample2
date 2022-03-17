using System;
using System.Collections.Generic;
using ApiObjects.Pricing;
using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class SubscriptionValidator
    {
        public static void ValidateForAdd(this KalturaSubscription model)
        {
            if (!string.IsNullOrEmpty(model.ChannelsIds))
            {
                _ = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.ChannelsIds, "channelsIds", true);
            }

            if (model.CouponGroups?.Count > 0)
            {
                model.CouponGroups.ForEach(x => x.Validate());
            }

            if (model.Name == null || model.Name.Values == null || model.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
            model.Name.Validate("multilingualName");

            if (model.Description != null && model.Description.Values != null && model.Description.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
            }

            if (model.Description != null)
            {
                model.Description.Validate("multilingualDescription");
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.StartDate > model.EndDate)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDate", "endDate");
            }

            if (!string.IsNullOrEmpty(model.FileTypesIds))
            {
                _ = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.FileTypesIds, "fileTypesIds", true);
            }

            if (model.ProductCodes?.Count > 1)
            {
                List<string> res = new List<string>();

                foreach (var item in model.ProductCodes)
                {
                    if (res.Contains(item.InappProvider))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "KalturaProductCode.InappProvider");
                    }

                    if (!Enum.TryParse(item.InappProvider, out VerificationPaymentGateway tmp))
                    {
                        throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "KalturaProductCode.InappProvider", item.InappProvider);
                    }

                    res.Add(item.InappProvider);
                }
            }
        }
    }
}