using ApiObjects.Base;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class CampaignIdInFilterValidator
    {
        public static void Validate(this KalturaCampaignIdInFilter model)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var items = model.GetItemsIn<List<long>, long>(model.IdIn, "idIn", true);
            if (items.Count > 500)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaCampaignIdInFilter.idIn", 500);
            }
        }
    }
}