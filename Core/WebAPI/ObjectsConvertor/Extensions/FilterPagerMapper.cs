using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterPagerMapper
    {
        public static int GetRealPageIndex(this KalturaFilterPager model)
        {
            return model.PageIndex.Value - 1;
        }
    }
}