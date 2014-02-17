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

        abstract public void NotifyChange(string sSubscriptionID, bool update);

        protected Int32 m_nGroupID;
    }
}
