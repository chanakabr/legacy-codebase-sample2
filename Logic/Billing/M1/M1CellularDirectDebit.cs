using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using DAL;
using KLogMonitor;
using M1BL;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class M1CellularDirectDebit : BaseCellularDirectDebit
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public M1CellularDirectDebit(int groupID)
            : base(groupID)
        {
        }

        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters)
        {
            throw new NotImplementedException();
        }
    }
}
