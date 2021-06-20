namespace ApiObjects.Pricing
{
    // TODO move to DAL/DTO
    public class PriceDTO
    {
        public double Price { get; set; }
        public CurrencyDTO Currency { get; set; }
        public int CountryId { get; set; }
    }
}