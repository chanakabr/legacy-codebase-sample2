using System.Collections.Generic;
using ApiObjects;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterFileByStreamerTypeActionMapper
    {
        public static List<StreamerType> GetStreamerTypes(this KalturaFilterFileByStreamerTypeAction model)
        {
            var streamerTypes = model.GetItemsIn<List<KalturaMediaFileStreamerType>, KalturaMediaFileStreamerType>(model.StreamerTypeIn, "streamerTypeIn", true, true);
            return AutoMapper.Mapper.Map<List<StreamerType>>(streamerTypes);
        }
    }
}