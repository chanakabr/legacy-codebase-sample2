using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class BillingResponse
    {

        public BillingResponseStatus m_oStatus { get; set; }
        public string m_sRecieptCode { get; set; }
        public string m_sStatusDescription { get; set; }
        public string m_sExternalReceiptCode { get; set; }
    }

    public enum BillingResponseStatus
    {
        /// <remarks/>
        Success,

        /// <remarks/>
        Fail,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        PriceNotCorrect,

        /// <remarks/>
        UnKnownUser,

        /// <remarks/>
        UnKnownPPVModule,
    }
}
