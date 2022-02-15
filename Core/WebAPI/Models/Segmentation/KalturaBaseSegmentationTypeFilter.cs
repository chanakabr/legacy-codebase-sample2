using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    public abstract partial class KalturaBaseSegmentationTypeFilter : KalturaFilter<KalturaSegmentationTypeOrder>
    {
        internal abstract KalturaSegmentationTypeListResponse GetSegmentationTypes(int groupId, long userId, KalturaFilterPager pager);

        public override KalturaSegmentationTypeOrder GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrder.NONE;
        }
    }
}
