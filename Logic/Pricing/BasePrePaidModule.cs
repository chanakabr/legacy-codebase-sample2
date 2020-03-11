using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public abstract class BasePrePaidModule
    {

        protected static readonly string BASE_PP_MODULE_LOG_FILE = "BasePrePaidModule";

        protected int m_GroupID;

        protected BasePrePaidModule() { }
        protected BasePrePaidModule(int nGroupID)
        {
            m_GroupID = nGroupID;
        }

        public int GroupID
        {
            get
            {
                return m_GroupID;
            }
            protected set
            {
                m_GroupID = value;
            }
        }

        public abstract PrePaidModule GetPrePaidModuleData(int nPrePaidModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
    }
}
