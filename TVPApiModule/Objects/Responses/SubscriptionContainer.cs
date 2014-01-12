using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class SubscriptionContainer
    {
        public string m_sSubscriptionCode { get; set; }

        public int m_nMaxUses { get; set; }

        public int m_nCurrentUses { get; set; }

        public DateTime m_dEndDate { get; set; }

        public DateTime m_dCurrentDate { get; set; }

        public DateTime m_dLastViewDate { get; set; }

        public DateTime m_dPurchaseDate { get; set; }

        public DateTime m_dNextRenewalDate { get; set; }

        public bool m_bRecurringStatus { get; set; }

        public bool m_bIsSubRenewable { get; set; }

        public int m_nSubscriptionPurchaseID { get; set; }

        public PaymentMethod m_paymentMethod { get; set; }

        public string m_sDeviceUDID { get; set; }

        public string m_sDeviceName { get; set; }
    }

    public enum PaymentMethod
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        CreditCard,

        /// <remarks/>
        SMS,

        /// <remarks/>
        PayPal,

        /// <remarks/>
        DebitCard,

        /// <remarks/>
        Ideal,

        /// <remarks/>
        Incaso,

        /// <remarks/>
        Gift,

        /// <remarks/>
        Visa,

        /// <remarks/>
        MasterCard,

        /// <remarks/>
        InApp,
    }
}
