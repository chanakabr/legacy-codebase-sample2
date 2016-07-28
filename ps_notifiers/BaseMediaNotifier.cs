using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ps_notifiers
{
    public abstract class BaseMediaNotifier
    {
        protected int m_GroupID;

        protected BaseMediaNotifier() { }

        protected BaseMediaNotifier(int groupID)
        {
            m_GroupID = groupID;
        }

        abstract public bool NotifyIngest(int mediaID);
    }
}
