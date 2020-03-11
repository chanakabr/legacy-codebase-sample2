using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;

namespace Financial
{
    public class Limitations
    {
        protected Int32 m_nGroupID;
        protected Int32 m_nLimitationID;

        protected string m_sName;
        protected string m_sDescription;

        protected Int32 m_nFinancialEntityID;
        protected Int32 m_nCurrencyCD;

        protected DateTime m_dStartDate;
        protected DateTime m_dEndDate;

        protected double m_nMinFixPrice;
        protected double m_nMaxFixPrice;

        public Limitations()
        {
            m_sName = string.Empty;
            m_sDescription = string.Empty;

            m_nGroupID = 0;
            m_nLimitationID = 0;

            m_nFinancialEntityID = 0;
            m_nCurrencyCD = 0;

            m_nMinFixPrice = 0;
            m_nMaxFixPrice = 0;

        }

        public void Initialize(Int32 nGroupID, Int32 nLimitationID, string sName, string sDescription, 
            Int32 nFinancialEntityID, Int32 nCurrencyCD, DateTime dStartDate, DateTime dEndDate,
            double nMinFixPrice, double nMaxFixPrice)
        {
            m_nLimitationID = nLimitationID;
            m_nGroupID = nGroupID;

            m_sName = sName;
            m_sDescription = sDescription;

            m_nFinancialEntityID = nFinancialEntityID;
            m_nCurrencyCD = nCurrencyCD;

            m_nMinFixPrice = nMinFixPrice;
            m_nMaxFixPrice = nMaxFixPrice;
        }
    }
}
