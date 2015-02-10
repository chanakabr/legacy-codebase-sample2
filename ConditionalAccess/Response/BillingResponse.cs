using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class BillingResponse
    {
        public BillingResponse()
        {
            m_oStatus = TvinciBilling.BillingResponseStatus.UnKnown;
            m_sRecieptCode = "";
            m_sStatusDescription = "";
        }

        public BillingResponse(TvinciBilling.BillingResponse toCopy)
        {
            m_oStatus = (TvinciBilling.BillingResponseStatus)(toCopy.m_oStatus);
            m_sRecieptCode = toCopy.m_sRecieptCode;
            m_sStatusDescription = toCopy.m_sStatusDescription;
            m_sExternalReceiptCode = toCopy.m_sExternalReceiptCode;
        }

        public TvinciBilling.BillingResponseStatus m_oStatus;
        public string m_sRecieptCode;
        public string m_sStatusDescription;
        public string m_sExternalReceiptCode;
       
    }
    public class InAppBillingResponse
    {
        public BillingResponse m_oBillingResponse;
        public InAppReceipt m_oInAppReceipt;
        public InAppBillingResponse(TvinciBilling.InAppBillingResponse toCopy)
        {
            m_oBillingResponse = new BillingResponse();
            m_oInAppReceipt = new InAppReceipt();
        }
    }
}
