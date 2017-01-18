using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class MediaAdObject
    {
        public MediaAdObject() 
        {
            m_sType = "";
            m_bSkip = false;
            m_nDelta = 0;
            m_sImplType = "";
            m_sUrl = "";
            m_dStartSec = 0.0;
            m_sPoints = "";
        }

        public void Initialize(string sType, bool bSkip, Int32 nDelta, string sImplType, string sURL, double dStartSec , string sPoints)
        {
            m_sPoints = sPoints;
            m_sType = sType;
            m_bSkip = bSkip;
            m_nDelta = nDelta;
            m_sImplType = sImplType;
            m_sUrl = sURL;
            m_dStartSec = dStartSec;
        }

        public string m_sType;
        public bool m_bSkip;
        public Int32 m_nDelta;
        public string m_sImplType;
        public string m_sUrl;
        public double m_dStartSec;
        public string m_sPoints;
    }
}
