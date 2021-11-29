using System.Collections.Generic;
using ApiObjects;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterFileByQualityActionMapper
    {
        public static List<MediaFileTypeQuality> GetQualities(this KalturaFilterFileByQualityAction model)
        {
            var types = model.GetItemsIn<List<KalturaMediaFileTypeQuality>, KalturaMediaFileTypeQuality>(model.QualityIn, "qualityIn", true, true);
            return AutoMapper.Mapper.Map<List<MediaFileTypeQuality>>(types);
        }
    }
}