using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class PriceCode
    {
        public string code { get; set; }

        public Price prise { get; set; }

        public int objectID { get; set; }

        public LanguageContainer[] description { get; set; }
    }

}
