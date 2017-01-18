using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Billing
{
    public abstract class BaseBilling
    {
        protected int m_nGroupID;
         public BaseBilling() { }

         public BaseBilling(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

         public abstract string BillingCustomData();


       
    }
}
