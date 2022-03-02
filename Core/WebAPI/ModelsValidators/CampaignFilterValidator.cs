using ApiObjects.Base;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class CampaignFilterValidator
    {
        public static void Validate(this KalturaCampaignFilter model, ContextData contextData)
        {
            switch (model)
            {
                case KalturaCampaignIdInFilter c: c.Validate(); break;
                default:
                    bool isAllowedToViewInactiveCampaigns = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.ToString(), true);
                    if (!isAllowedToViewInactiveCampaigns)
                    {
                        throw new UnauthorizedException(UnauthorizedException.SERVICE_FORBIDDEN);
                    }
                    break;
            }
        }

        private static void Validate(this KalturaCampaignIdInFilter model)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var items = Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.IdIn, "idIn", true);
            if (items.Count > 500)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaCampaignIdInFilter.idIn", 500);
            }
        }
    }
}