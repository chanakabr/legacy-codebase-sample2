using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    public abstract partial class KalturaBaseSegmentationTypeFilter : KalturaFilter<KalturaSegmentationTypeOrder>
    {
        public override KalturaSegmentationTypeOrder GetDefaultOrderByValue()
        {
            return KalturaSegmentationTypeOrder.NONE;
        }
    }
}
