using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses.Pricing
{
    public class Price
    {

        [JsonProperty(PropertyName = "Price")]
        public double price;

        [JsonProperty(PropertyName = "Currency")]
        public Currency Currency;

        public Price()
        {
            price = 0.0;
            Currency = new Currency();
        }

        public Price(TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.Price sourcePrice)
        {            
            this.price = sourcePrice.m_dPrice;
            this.Currency = new Currency(sourcePrice.m_oCurrency);
        }

        public void InitializeByCD3(string sCurrencyCode3, double dP)
        {
            Currency.InitializeByCode3(sCurrencyCode3);
            price = dP;
        }

        public void InitializeByCodeID(Int32 nCurrencyID, double dP)
        {
            Currency.InitializeById(nCurrencyID);
            price = dP;
        }

        public void InitializeByCD2(string sCurrencyCode2, double dP)
        {
            Currency.InitializeByCode2(sCurrencyCode2);
            price = dP;
        }    
    }
}