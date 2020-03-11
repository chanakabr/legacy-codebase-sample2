using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class BillingResponse
    {
        public BillingResponse()
        {
            m_oStatus = BillingResponseStatus.UnKnown;
            m_sRecieptCode = "";
            m_sStatusDescription = "";
        }

        public void Initialize(BillingResponseStatus oStatus, string sRecieptCode, string sStatusDescription)
        {
            m_oStatus = oStatus;
            m_sRecieptCode = sRecieptCode;
            m_sStatusDescription = sStatusDescription;
        }

        public BillingResponseStatus m_oStatus;
        public string m_sRecieptCode;
        public string m_sStatusDescription;
        public string m_sExternalReceiptCode;

    }
    public class InAppBillingResponse
    {
        public BillingResponse m_oBillingResponse;
        public InAppReceipt m_oInAppReceipt;
        public InAppBillingResponse()
        {
            m_oBillingResponse = new BillingResponse();
            m_oInAppReceipt = new InAppReceipt();    
        }
    }

}
