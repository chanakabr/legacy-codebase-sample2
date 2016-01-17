using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.Pricing
{
    public class Price
    {

        [JsonProperty(PropertyName = "price")]
        public double price;

        [JsonProperty(PropertyName = "currency")]
        public Currency currency;

        public Price()
        {
            price = 0.0;
            currency = new Currency();
        }

        public Price(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Price sourcePrice)
        {            
            this.price = sourcePrice.m_dPrice;
            this.currency = new Currency(sourcePrice.m_oCurrency);
        }

        public void InitializeByCD3(string sCurrencyCode3, double dP)
        {
            currency.InitializeByCode3(sCurrencyCode3);
            price = dP;
        }

        public void InitializeByCodeID(Int32 nCurrencyID, double dP)
        {
            currency.InitializeById(nCurrencyID);
            price = dP;
        }

        public void InitializeByCD2(string sCurrencyCode2, double dP)
        {
            currency.InitializeByCode2(sCurrencyCode2);
            price = dP;
        }    
    }
}