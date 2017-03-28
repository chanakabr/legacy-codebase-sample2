using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class CountryLocaleResponse : CountryResponse
    {
        
        public List<CountryLocale> CountryLocales { get; set; }

        public CountryLocaleResponse()
            : base()
        {            
            this.CountryLocales = new List<CountryLocale>();
        }

    }
}
