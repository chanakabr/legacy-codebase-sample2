using ApiObjects.Base;

namespace ApiObjects.Segmentation
{
    public class HouseholdSegmentFilter : ICrudFilter
    {
        public string Ksql { get; set; }
    }
}