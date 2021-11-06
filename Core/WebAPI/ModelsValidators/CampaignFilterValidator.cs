using ApiObjects.Base;
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
    }
}