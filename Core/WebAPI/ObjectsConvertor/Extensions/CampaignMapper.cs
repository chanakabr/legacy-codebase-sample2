using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class CampaignMapper
    {
        public static List<long> GetCollectionIds(this KalturaCampaign model)
        {
            if (model.CollectionIdIn == null) { return null; }
            return model.GetItemsIn<List<long>, long>(model.CollectionIdIn, "collectionIdIn");
        }
    }
}