using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class PreviewModule
    {
        public long m_nID;
        public string m_sName;
        public Int32 m_tsFullLifeCycle;
        public Int32 m_tsNonRenewPeriod;
        public string alias;

        public PreviewModule()
        {
            m_nID = 0;
            m_sName = string.Empty;
            m_tsFullLifeCycle = 0;
            m_tsNonRenewPeriod = 0;
            alias = string.Empty;
        }

        public PreviewModule(long nID, string sName, int nFullLifeCycle, int nNonRenewingPeriod)
        {
            this.m_nID = nID;
            this.m_sName = sName;
            this.m_tsFullLifeCycle = nFullLifeCycle;
            this.m_tsNonRenewPeriod = nNonRenewingPeriod;
        }

        public PreviewModule(long nID, string sName, int nFullLifeCycle, int nNonRenewingPeriod, string alias)
        {
            this.m_nID = nID;
            this.m_sName = sName;
            this.m_tsFullLifeCycle = nFullLifeCycle;
            this.m_tsNonRenewPeriod = nNonRenewingPeriod;
            this.alias = alias;
        }

        public bool IsNeedToUpdate(PreviewModule oldPv)
        {
            bool needToUpdate = false;

            if (!string.IsNullOrEmpty(m_sName) && !m_sName.Equals(oldPv.m_sName))
            {
                needToUpdate = true;
            }
            else
            {
                m_sName = oldPv.m_sName;
            }

            if (m_tsFullLifeCycle > 0 && m_tsFullLifeCycle != oldPv.m_tsFullLifeCycle)
            {
                needToUpdate = true;
            }
            else
            {
                m_tsFullLifeCycle = oldPv.m_tsFullLifeCycle;
            }

            if (m_tsNonRenewPeriod > 0 && m_tsNonRenewPeriod != oldPv.m_tsNonRenewPeriod)
            {
                needToUpdate = true;
            }
            else
            {
                m_tsNonRenewPeriod = oldPv.m_tsNonRenewPeriod;
            }

            return needToUpdate;
        }
    }

}
