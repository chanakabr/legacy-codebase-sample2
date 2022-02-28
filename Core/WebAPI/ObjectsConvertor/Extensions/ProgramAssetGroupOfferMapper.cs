using System.Collections.Generic;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ProgramAssetGroupOfferMapper
    {
        public static List<long> GetProgramAssetGroupOfferIds(this KalturaProgramAssetGroupOfferIdInFilter model)
        {
            if (model.IdIn == null) { return null; }
            return model.GetItemsIn<List<long>, long>(model.IdIn, "idIn");
        }
    }
}