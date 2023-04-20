using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Protobuf;

namespace Core.Pricing
{
    // TODO delete IPrice in task https://kaltura.atlassian.net/browse/BEO-10405 and use IGeneralPartnerConfigManager.GetCurrencyList instead!!
    public interface IPrice
    {
        Price InitializeByCD3(string sCurrencyCode3, double dP);
    }

    [Serializable]
    public class Price : IPrice, IDeepCloneable<Price>
    {
        private static readonly Lazy<Price> lazy = new Lazy<Price>(() => new Price(), LazyThreadSafetyMode.PublicationOnly);
        public static Price Instance { get { return lazy.Value; } }

        public double m_dPrice;
        public Currency m_oCurrency;
        public int countryId;

        public Price()
        {
            m_dPrice = 0.0;
            m_oCurrency = new Currency();
            countryId = 0;
        }

        public Price(Price item)
        {
            m_dPrice = item.m_dPrice;
            m_oCurrency = new Currency(item.m_oCurrency);
            countryId = item.countryId;
        }

        // TODO - need to delete all InitializeBy methods and use IGeneralPartnerConfigManager.GetCurrencyList instead!!
        public Price InitializeByCD3(string sCurrencyCode3, double dP)
        {
            m_oCurrency.InitializeByCode3(sCurrencyCode3);
            m_dPrice = dP;
            return this;
        }

        public void InitializeByCodeID(Int32 nCurrencyID, double dP)
        {
            m_oCurrency.InitializeById(nCurrencyID);
            m_dPrice = dP;
        }

        public void InitializeByCD2(string sCurrencyCode2, double dP)
        {
            m_oCurrency.InitializeByCode2(sCurrencyCode2);
            m_dPrice = dP;
        }

        public bool IsFree()
        {
            return m_dPrice == 0.0;
        }

        public Price Clone()
        {
            return new Price(this);
        }
    }

    public class FullPrice
    {
        public Price OriginalPrice { get; set; }
        public Price FinalPrice { get; set; }
        public ApiObjects.ConditionalAccess.PriceReason PriceReason { get; set; }
        public string CouponCode { get; set; }
        public double CouponRemainder { get; set; }
        public ApiObjects.Billing.SubscriptionCycle SubscriptionCycle { get; set; }
        public ApiObjects.ConditionalAccess.RecurringCampaignDetails CampaignDetails { get; set; }
    }
}
