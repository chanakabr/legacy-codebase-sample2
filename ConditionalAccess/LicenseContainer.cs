using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class LicenseListResponse
    {
        public LicenseContainer[] m_Licenses;
        public Int32 m_nLicenseCount;
        public LicenseListResponse()
        {
            m_Licenses = null;
            m_nLicenseCount = 0;
        }
    }
    
    public class LicenseContainer
    {
        public string m_sRecieptCode;
        public DateTime m_dtActionDate;

        public LicenseContainer()
        {
            m_sRecieptCode = "";
            m_dtActionDate = new DateTime(2000, 1, 1);
        }

        public void Initialize(string sRecieptCode, DateTime dtActionDate)
        {
            m_sRecieptCode = sRecieptCode;
            m_dtActionDate = new DateTime(2000, 1, 1);
        }
    }
}
