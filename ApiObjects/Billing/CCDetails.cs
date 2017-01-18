using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class CCDetails
    {
        public CCDetails() 
        {
            m_sCardHolderName = "";
            m_dExpiration = new DateTime(2000, 1, 1);
            m_sCVV = "";
            m_sCardNumber = "";
        }

        public void Initialize(string sCardNumber, string sCVV, DateTime dExpiration, string sCardHolderName)
        {
            m_sCardNumber = sCardNumber;
            m_sCardHolderName = sCardHolderName;
            m_sCVV = sCVV;
            m_dExpiration = dExpiration;
        }

        public string m_sCardNumber;
        public string m_sCVV;
        public DateTime m_dExpiration;
        public string m_sCardHolderName;
    }
}
