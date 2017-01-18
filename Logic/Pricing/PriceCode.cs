using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class PriceCode
    {
        public string m_sCode;
        public Price m_oPrise;
        public Int32 m_nObjectID;
        public LanguageContainer[] m_sDescription;

        public PriceCode()
        {
            m_sCode = string.Empty;
            m_oPrise = new Price();
            m_nObjectID = 0;
        }

        public bool Initialize(string sC, Price p, LanguageContainer[] sD, Int32 nPriceCodeID)
        {
            m_sCode = sC;
            m_oPrise = p;
            m_sDescription = sD;
            m_nObjectID = nPriceCodeID;
            return true;
        }

        public override string ToString()
        {
            return String.Concat("PriceCode: ", m_sCode);
        }

    }
}
