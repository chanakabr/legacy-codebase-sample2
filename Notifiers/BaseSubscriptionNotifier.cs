using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifiers
{
    public abstract class BaseSubscriptionNotifier
    {
        protected BaseSubscriptionNotifier() { }

        protected BaseSubscriptionNotifier(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        abstract public void NotifyChange(string sSubscriptionID);

        abstract public void NotifyChange(string sSubscriptionID, int create0update1assign2);

        abstract public void NotifyChange(string sSubscriptionID, ref string errorMessage, int create0update1assign2);

        protected Int32 m_nGroupID;
    }
}
