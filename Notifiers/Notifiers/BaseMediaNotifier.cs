using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Notifiers
{
    public abstract class BaseMediaNotifier
    {
        protected BaseMediaNotifier() { }

        protected BaseMediaNotifier(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        abstract public void NotifyChange(string sSiteGUID);

        protected Int32 m_nGroupID;
    }
}
