using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public abstract class BaseCellularCreditCard
    {
        protected Int32 m_nGroupID;

        protected BaseCellularCreditCard() { }

        protected BaseCellularCreditCard(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;    
        }

        public abstract BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters);

        public abstract bool UpdatePurchaseIDInBillingTable(long lPurchaseID, long billingRefTransactionID);

    }
}
