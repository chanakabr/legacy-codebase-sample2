using ApiObjects.Response;
using Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class BillingTransactionsResponse
    {
        public BillingTransactionContainer[] m_Transactions;
        public Int32 m_nTransactionsCount;
        public BillingTransactionsResponse()
        {     
            m_Transactions = null;
            m_nTransactionsCount = 0;
        }
    }

    public class BillingTransactionContainer
    {
        public string m_sRecieptCode;
        public string m_sPurchasedItemName;
        public string m_sPurchasedItemCode;
        public BillingItemsType m_eItemType;
        public BillingAction m_eBillingAction;
        public Price m_Price;
        public DateTime m_dtActionDate;
        public DateTime m_dtStartDate;
        public DateTime m_dtEndDate;
        public PaymentMethod m_ePaymentMethod;
        public string m_sPaymentMethodExtraDetails;
        public bool m_bIsRecurring;
        public Int32 m_nBillingProviderRef;
        public Int32 m_nPurchaseID;
        public string m_sRemarks;

        public BillingTransactionContainer()
        {
            m_sRemarks = "";
            m_nPurchaseID = 0;
            m_nBillingProviderRef = 0;
            m_sRecieptCode = "";
            m_sPurchasedItemName = "";
            m_sPurchasedItemCode = "";
            m_eItemType = BillingItemsType.Unknown;
            m_eBillingAction = BillingAction.Unknown;
            m_Price = null;
            m_dtActionDate = new DateTime(2000, 1, 1);
            m_dtStartDate = new DateTime(2000, 1, 1);
            m_dtEndDate = new DateTime(2000, 1, 1);
            m_ePaymentMethod = PaymentMethod.Unknown;
            m_sPaymentMethodExtraDetails = "";
            m_bIsRecurring = false;
        }

        public void Initialize(string sRecieptCode, string sPurchasedItemName, string sPurchasedItemCode ,
            BillingItemsType eItemType, BillingAction eBillingAction , double dPrice , string sCurrency ,
            DateTime dtActionDate, DateTime dtStartDate, DateTime dtEndDate, PaymentMethod ePaymentMethod,
            string sPaymentMethodExtraDetails, bool bIsRecurring, Int32 nPurchaseID, Int32 nBillingProviderRef , 
            string sRemark)
        {
            m_sRemarks = sRemark;
            m_nPurchaseID = nPurchaseID;
            m_nBillingProviderRef = nBillingProviderRef;
            m_bIsRecurring = bIsRecurring;
            m_sRecieptCode = sRecieptCode;
            m_sPurchasedItemName = sPurchasedItemName;
            m_sPurchasedItemCode = sPurchasedItemCode;
            m_eItemType = eItemType;
            m_eBillingAction = eBillingAction;
            m_Price = new Price();
            m_Price.m_dPrice = dPrice;
            m_Price.m_oCurrency = new Currency();
            //m_Price.m_oCurrency.
            m_dtActionDate = new DateTime(2000, 1, 1);
            m_dtStartDate = new DateTime(2000, 1, 1);
            m_ePaymentMethod = PaymentMethod.Unknown;
            m_sPaymentMethodExtraDetails = "";
        }
    }

    public class TransactionHistoryContainer : BillingTransactionContainer
    {
        public string SiteGuid;

        public string UserFullName;

        public TransactionHistoryContainer() : base()
        {            
        }
    }

    public class PrePaidHistoryResponse
    {
        public PrePaidHistoryContainer[] m_Transactions;
        public Int32 m_nTransactionsCount;
        
        public PrePaidHistoryResponse()
        {
            m_Transactions = null;
            m_nTransactionsCount = 0;
        }
    }

    public class PrePaidHistoryContainer
    {
        public string m_sPurchasedItemName;
        public string m_sPurchasedItemCode;
        public BillingItemsType m_eItemType;
        public Price m_oPrice;
        public Price m_oCredit;
        public DateTime m_dtActionDate;

        public PrePaidHistoryContainer()
        {
            
            m_sPurchasedItemName = "";
            m_sPurchasedItemCode = "";
            m_eItemType = BillingItemsType.Unknown;
            
            m_oPrice = null;
            m_oCredit = null;
            m_dtActionDate = new DateTime(2000, 1, 1);
        }
    }
}
