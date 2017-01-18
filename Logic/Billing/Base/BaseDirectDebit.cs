using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public abstract class BaseDirectDebit
    {

        protected int m_nGroupID;

        public BaseDirectDebit() { }

        public BaseDirectDebit(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, Int32 nPaymentNumber, Int32 nNumberOfPayments, string sExtraParameters, int nBillingMethod);

        public abstract bool RefundPayment(string sPSPReference, string sSiteGuid, int nGroupID, double dChargePrice, string sCurrencyCode, long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);

        public abstract bool CancelPayment(string sPSPReference, string sMerchantAccount, string sSiteGuid, int nGroupID, long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne, double? dChargePrice, string sCurrencyCode);

        public virtual AdyenBillingDetail GetLastBillingUserInfo(string sSiteGiuid, int nBillingMethod)
        {
            return new AdyenBillingDetail();
        }

        public abstract bool CancelOrRefundPayment(string sPSPReference, string sSiteGuid, double? dPrice, string sCurrencyCode, long lPurchaseID, int nType, bool bIsCancelOrRefundResultOfPreviewModule, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
    }
}
