using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PriceCode
    {
        public string code { get; set; }

        public Price price { get; set; }

        public int object_id { get; set; }

        public LanguageContainer[] description { get; set; }
    }

}
