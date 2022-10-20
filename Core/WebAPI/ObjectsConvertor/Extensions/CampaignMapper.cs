using ApiObjects;
using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class CampaignMapper
    {
        public static List<long> GetCollectionIds(this KalturaCampaign model)
        {
            if (model.CollectionIdIn == null) { return null; }
            return Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.CollectionIdIn, "collectionIdIn");
        }
    }

    public static class CampaignSearchFilterMapper
    {
        public static List<CampaignState> GetStates(this KalturaCampaignSearchFilter model)
        {
            var streamerTypes = Utils.Utils.ParseCommaSeparatedValues<List<KalturaObjectState>, KalturaObjectState>(model.StateIn, "stateIn", true, true);
            return AutoMapper.Mapper.Map<List<CampaignState>>(streamerTypes);
        }
    }
}
