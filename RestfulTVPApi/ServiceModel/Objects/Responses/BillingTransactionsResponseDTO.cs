using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class BillingTransactionsResponseDTO
    {
        public BillingTransactionContainerDTO[] m_Transactions { get; set; }
        public int m_nTransactionsCount { get; set; }
    }

    public class BillingTransactionContainerDTO 
    {
        public string m_sRecieptCode { get; set; }
        public string m_sPurchasedItemName { get; set; }
        public string m_sPurchasedItemCode { get; set; }
        public BillingItemsTypeDTO m_eItemType { get; set; }
        public BillingActionDTO m_eBillingAction { get; set; }
        public PriceDTO m_Price { get; set; }
        public DateTime m_dtActionDate { get; set; }
        public DateTime m_dtStartDate { get; set; }
        public DateTime m_dtEndDate { get; set; }
        public PaymentMethodDTO m_ePaymentMethod { get; set; }
        public string m_sPaymentMethodExtraDetails { get; set; }
        public bool m_bIsRecurring { get; set; }
        public int m_nBillingProviderRef { get; set; }
        public int m_nPurchaseID { get; set; }
        public string m_sRemarks { get; set; }
    }

    public enum BillingItemsTypeDTO
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        PPV,

        /// <remarks/>
        Subscription,

        /// <remarks/>
        PrePaid,

        /// <remarks/>
        PrePaidExpired,
    }

    public enum BillingActionDTO
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        Purchase,

        /// <remarks/>
        RenewPayment,

        /// <remarks/>
        RenewCancledSubscription,

        /// <remarks/>
        CancelSubscriptionOrder,

        /// <remarks/>
        SubscriptionDateChanged,
    }

    public class PriceDTO
    {
        public double m_dPrice { get; set; }
        public CurrencyDTO m_oCurrency { get; set; }
    }

    public class CurrencyDTO
    {
        public string m_sCurrencyCD3 { get; set; }
        public string m_sCurrencyCD2 { get; set; }
        public string m_sCurrencySign { get; set; }
        public int m_nCurrencyID { get; set; }
    }
}