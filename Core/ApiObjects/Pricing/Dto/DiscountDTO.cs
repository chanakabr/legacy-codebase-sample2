namespace ApiObjects.Pricing
{
    // TODO move to DAL/DTO
    public class DiscountDTO
    {
        public double Price { get; }
        public double Percentage { get; }
        public int CurrencyId { get; }
        public int CountryId { get; }

        public DiscountDTO(double price, double percentage, int сurrencyId, int countryId)
        {
            CountryId = countryId;
            CurrencyId = сurrencyId;
            Percentage = percentage;
            Price = price;
        }
    }
}
