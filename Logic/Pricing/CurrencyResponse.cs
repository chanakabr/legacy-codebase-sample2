using ApiObjects.Response;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    public class CurrencyResponse
    {

        public Status Status;

        public List<Currency> Currencies { get; set; }

        public CurrencyResponse()
        {
            this.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.Currencies = new List<Currency>();
        }

    }
}
