using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using System.Net;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class ElisaInAppPurchase : BaseInAppPurchase
    {

        public ElisaInAppPurchase(int groupID)
            : base(groupID)
        {
        }

        public override InAppBillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string ReceiptData)
        {
            return base.ChargeUser(sSiteGUID, dChargePrice, sCurrencyCode, sUserIP, sCustomData, nPaymentNumber, nNumberOfPayments, ReceiptData);
        }
        public override InAppBillingResponse ReneweInAppPurchase(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sCustomData, int nPaymentNumber, int nNumberOfPayments, int nInAppTransactionID)
        {
            return base.ReneweInAppPurchase(sSiteGUID, dChargePrice, sCurrencyCode, sCustomData, nPaymentNumber, nNumberOfPayments, nInAppTransactionID);
        }      
        
    }
}
