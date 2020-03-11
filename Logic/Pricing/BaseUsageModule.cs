using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BaseUsageModule
    {
        protected static readonly string BASE_UM_LOG_FILE = "BaseUsageModule";
        protected Int32 m_nGroupID;

        protected BaseUsageModule() { }
        protected BaseUsageModule(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public int GroupID
        {
            get
            {
                return m_nGroupID;
            }
            protected set
            {
                m_nGroupID = value;
            }
        }

        public abstract UsageModule GetUsageModuleData(string sUsageModuleCode);
        public abstract UsageModule[] GetUsageModuleList();
        public abstract UsageModule[] GetSubscriptionUsageModuleList(string nSubscitionnSubscriptionCode);
        public abstract UsageModule GetOfflineUsageModuleData();
        
    }
}
