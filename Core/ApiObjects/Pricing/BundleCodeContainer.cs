using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Google.Protobuf;

namespace ApiObjects.Pricing
{
    [Serializable]
    public class BundleCodeContainer : IDeepCloneable<BundleCodeContainer>
    {
        public BundleCodeContainer()
        {
            m_sCode = string.Empty;
            m_sName = string.Empty;
        }
        
        public BundleCodeContainer(BundleCodeContainer other) {
            m_sCode = other.m_sCode;
            m_sName = other.m_sName;
        }
        
        public void Initialize(string sCode, string sName)
        {
            m_sCode = sCode;
            m_sName = sName;
        }
        
        public BundleCodeContainer Clone()
        {
            return new BundleCodeContainer(this);
        }
        
        public string m_sCode { get; set; }
        public string m_sName;
    }
}
