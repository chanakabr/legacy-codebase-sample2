using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    class _999SMS : BaseSMS
    {
        public _999SMS(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override BillingResponse CheckCode(string sSiteGUID, string sCellPhone, string sCode, string sReferenceCode)
        {
            BillingResponse ret = new BillingResponse();
            ret.m_oStatus = BillingResponseStatus.Success;
            ret.m_sRecieptCode = "";
            ret.m_sStatusDescription = "";
            return ret;
        }

        public override BillingResponse SendCode(string sSiteGUID, string sCellPhone, string sReferenceCode, string sExtraParameters)
        {
            BillingResponse ret = new BillingResponse();
            ret.m_oStatus = BillingResponseStatus.Success;
            ret.m_sRecieptCode = System.Guid.NewGuid().ToString();
            ret.m_sStatusDescription = "";
            return ret;
        }
    }
}
