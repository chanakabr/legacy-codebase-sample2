using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class UserIMRequestObject
    {
        public UserIMRequestObject() 
        {
            m_sTvinciGuid = "";
            m_sSiteGuid = "";
            m_sUserAgent = "";
            m_sUserIP = "";
        }

        public void Initialize(string sTvinciGuid, string sSiteGuid, string sUserAgent, string sUserIP)
        {
            m_sTvinciGuid = sTvinciGuid;
            m_sSiteGuid = sSiteGuid;
            m_sUserAgent = sUserAgent;
            m_sUserIP = sUserIP;
        }

        public string m_sTvinciGuid;
        public string m_sSiteGuid;
        public string m_sUserAgent;
        public string m_sUserIP;
    }
}
