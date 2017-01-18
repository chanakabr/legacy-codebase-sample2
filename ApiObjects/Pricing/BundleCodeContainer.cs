using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Pricing
{
    [Serializable]
    public class BundleCodeContainer
    {
        public BundleCodeContainer()
        {
            m_sCode = string.Empty;
            m_sName = string.Empty;
        }
        public void Initialize(string sCode, string sName)
        {
            m_sCode = sCode;
            m_sName = sName;
        }

        public string m_sCode;
        public string m_sName;
    }
}
