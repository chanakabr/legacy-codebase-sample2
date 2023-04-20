using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace Core.Pricing
{
    [Serializable]
    public class PreviewModule : IDeepCloneable<PreviewModule>
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
            m_nID = nID;
            m_sName = sName;
            m_tsFullLifeCycle = nFullLifeCycle;
            m_tsNonRenewPeriod = nNonRenewingPeriod;
        }

        public PreviewModule(long nID, string sName, int nFullLifeCycle, int nNonRenewingPeriod, string alias)
        {
            m_nID = nID;
            m_sName = sName;
            m_tsFullLifeCycle = nFullLifeCycle;
            m_tsNonRenewPeriod = nNonRenewingPeriod;
            this.alias = alias;
        }

        public PreviewModule(PreviewModule other) {
            m_nID = other.m_nID;
            m_sName = other.m_sName;
            m_tsFullLifeCycle = other.m_tsFullLifeCycle;
            m_tsNonRenewPeriod = other.m_tsNonRenewPeriod;
            alias = other.alias;
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

        public PreviewModule Clone()
        {
            return new PreviewModule(this);
        }
    }

}
