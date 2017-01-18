using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class Price
    {
        public double m_dPrice;
        public Currency m_oCurrency;

        public Price()
        {
            m_dPrice = 0.0;
            m_oCurrency = new Currency();
        }

        public void InitializeByCD3(string sCurrencyCode3, double dP)
        {
            m_oCurrency.InitializeByCode3(sCurrencyCode3);
            m_dPrice = dP;
        }

        public void InitializeByCodeID(Int32 nCurrencyID, double dP)
        {
            m_oCurrency.InitializeById(nCurrencyID);
            m_dPrice = dP;
        }

        public void InitializeByCD2(string sCurrencyCode2, double dP)
        {
            m_oCurrency.InitializeByCode2(sCurrencyCode2);
            m_dPrice = dP;
        }

       
    }
}
