using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterFileByVideoCodecActionMapper
    {
        public static List<string> GetVideoCodecs(this KalturaFilterFileByVideoCodecAction model)
        {
            return model.GetItemsIn<List<string>, string>(model.VideoCodecIn, "videoCodecIn", true);
        }
    }
}