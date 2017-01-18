using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public abstract class BaseCellularDirectDebit
    {
        protected Int32 m_nGroupID;

        protected BaseCellularDirectDebit() { }

        protected BaseCellularDirectDebit(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;    
        }

        public abstract BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters);

    }
}
