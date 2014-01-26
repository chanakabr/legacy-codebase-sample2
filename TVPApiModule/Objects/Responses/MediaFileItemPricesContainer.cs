using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class MediaFileItemPricesContainer
    {
        public int mediaFileID { get; set; }
       
        public ItemPriceContainer[] itemPrices { get; set; }
       
        public string productCode { get; set; }
    }
}
