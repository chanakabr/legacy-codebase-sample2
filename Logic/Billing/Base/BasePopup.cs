using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public abstract class BasePopup
    {
        protected BasePopup() { }

        protected BasePopup(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract string GetPopupMethodURL(double dChargePrice, string sCurrencyCode, string sItemName,
            string sCustomData, string sPaymentMethod, string sExtranParameters);

        protected Int32 m_nGroupID;
    }
}
