using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class  KalturaFilterFileByAudioCodecActionMapper
    {
        public static List<string> GetAudioCodecs(this KalturaFilterFileByAudioCodecAction model)
        {
            return model.GetItemsIn<List<string>, string>(model.AudioCodecIn, "audioCodecIn", true);
        }
    }
}