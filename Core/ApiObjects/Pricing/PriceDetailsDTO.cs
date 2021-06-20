using System.Collections.Generic;

namespace ApiObjects.Pricing
{
    public class PriceDetailsDTO
    {
        public string Name { get; set; }
        public long Id { get; set; }
        public List<PriceDTO> Prices { get; set; }
    }
}