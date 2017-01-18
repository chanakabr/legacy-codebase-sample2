using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MetaM2MObject : BaseCacheObject
    {
        public MetaM2MObject() 
        {
            m_sMetaName = "";
            m_sMetaValues = null;
            m_nMetaID = 0;
        }

        public void Initialize(string sMetaName, string[] sMetaValues)
        {
            m_sMetaName = sMetaName;
            m_sMetaValues = sMetaValues;
        }

        public override string GetCacheKey(int nMetaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMetaID.ToString();
            return sKey;
        }

        public string m_sMetaName;
        public string[] m_sMetaValues;
        public Int32 m_nMetaID;
    }

    public class MetaStrObject : BaseCacheObject
    {
        public MetaStrObject()
        {
            m_sMetaName = "";
            m_sMetaValue = "";
        }

        public void Initialize(string sMetaName, string sMetaValue)
        {
            m_sMetaName = sMetaName;
            m_sMetaValue = sMetaValue;
        }

        public override string GetCacheKey(int nMetaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMetaID.ToString();
            return sKey;
        }

        public string m_sMetaName;
        public string m_sMetaValue;
    }

    public class DoubleRange
    {
        public DoubleRange()
        {
            m_dMin = 0;
            m_dMax = 0;
        }

        public void Initialize(double dMin, double dMax)
        {
            m_dMin = dMin;
            m_dMax = dMax;
        }

        public double m_dMin;
        public double m_dMax;
    }

    public class StringRange
    {
        public StringRange()
        {
            m_sMin = "";
            m_sMax = "";
        }

        public void Initialize(string sMin, string sMax)
        {
            m_sMin = sMin;
            m_sMax = sMax;
        }

        public string m_sMin;
        public string m_sMax;
    }

    public class MetaDoubleObject : BaseCacheObject
    {
        public MetaDoubleObject()
        {
            m_sMetaName = "";
            m_dMetaValue = 0.0;
            m_oDoubleRange = null;
        }

        public void Initialize(string sMetaName, double dMetaValue , DoubleRange oDoubleRange)
        {
            m_sMetaName = sMetaName;
            m_dMetaValue = dMetaValue;
            m_oDoubleRange = oDoubleRange;
        }

        public override string GetCacheKey(int nMetaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMetaID.ToString();
            return sKey;
        }

        public string m_sMetaName;
        public double m_dMetaValue;
        public DoubleRange m_oDoubleRange;
    }

    public class MetaBoolObject : BaseCacheObject
    {
        public MetaBoolObject()
        {
            m_sMetaName = "";
            m_bMetaValue = false;
        }

        public void Initialize(string sMetaName, bool bMetaValue)
        {
            m_sMetaName = sMetaName;
            m_bMetaValue = bMetaValue;
        }

        public override string GetCacheKey(int nMetaID)
        {
            string sKey = this.GetType().ToString() + "_" + nMetaID.ToString();
            return sKey;
        }

        public string m_sMetaName;
        public bool m_bMetaValue;
    }
}
