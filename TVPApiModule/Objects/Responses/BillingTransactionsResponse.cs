using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class BillingTransactionsResponse
    {
        public BillingTransactionContainer[] m_Transactions { get; set; }
       
        public int m_nTransactionsCount { get; set; }
    }

    public class BillingTransactionContainer
    {
        public string m_sRecieptCode { get; set; }

        public string m_sPurchasedItemName { get; set; }
        
        public string m_sPurchasedItemCode { get; set; }
        
        public BillingItemsType m_eItemType { get; set; }
        
        public BillingAction m_eBillingAction { get; set; } 
        
        public Price m_Price { get; set; }
        
        public System.DateTime m_dtActionDate { get; set; }
       
        public System.DateTime m_dtStartDate { get; set; }
        
        public System.DateTime m_dtEndDate { get; set; }
        
        public PaymentMethod m_ePaymentMethod { get; set; }

        public string m_sPaymentMethodExtraDetails { get; set; }
       
        public bool m_bIsRecurring { get; set; }
       
        public int m_nBillingProviderRef { get; set; }
       
        public int m_nPurchaseID  { get; set; }
       
        public string m_sRemarks { get; set; } 
    }

    public class Price
    {
        public double m_dPrice { get; set; } 
           
        public Currency m_oCurrency { get; set; } 
    }

    public class Currency
    {
        public string m_sCurrencyCD3 { get; set; } 
       
        public string m_sCurrencyCD2 { get; set; } 

        public string m_sCurrencySign { get; set; } 
       
        public int m_nCurrencyID { get; set; } 
    }

    public enum BillingItemsType
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

    public enum BillingAction
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
    
}
