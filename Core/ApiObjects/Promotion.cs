namespace ApiObjects
{
    public class Promotion : BasePromotion
    {
        public long DiscountModuleId { get; set; }
        public int? NumberOfRecurring { get; set; }
        public int? MaxDiscountUsages { get; set; }

        public override int GetNumberOfRecurring()
        {
            return NumberOfRecurring ?? -1;
        }
    }
}