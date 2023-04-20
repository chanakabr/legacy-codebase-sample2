using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using APILogic;
using Google.Protobuf;

namespace Core.Pricing
{
    [Serializable]
    public class PriceCode : IDeepCloneable<PriceCode>
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

        public PriceCode(int objectID)
        {
            m_sCode = string.Empty;
            m_oPrise = new Price();
            m_nObjectID = objectID;
        }

        public PriceCode(PriceCode other) {
            m_sCode = other.m_sCode;
            m_oPrise = Extensions.Clone(other.m_oPrise);
            m_nObjectID = other.m_nObjectID;
            m_sDescription = Extensions.Clone(other.m_sDescription);
        }
        
        public bool Initialize(string sC, Price p, LanguageContainer[] sD, Int32 nPriceCodeID)
        {
            m_sCode = sC;
            m_oPrise = p;
            m_sDescription = sD;
            m_nObjectID = nPriceCodeID;
            return true;
        }

        public PriceCode Clone()
        {
            return new PriceCode(this);
        }

        public override string ToString()
        {
            return String.Concat("PriceCode: ", m_sCode);
        }
    }
}
