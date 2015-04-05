using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class MediaFileItemPricesContainer
    {
        public int media_file_id { get; set; }
       
        public ItemPriceContainer[] item_prices { get; set; }
       
        public string product_code { get; set; }
    }
}
