using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCGroupRules;
using MCGroupRules.Implementations;

namespace Core.Users
{
    public class MCMailImpl : BaseMailImpl
    {
        private MCWelcomeMail m_oMCMAIL;

        public MCMailImpl(int nGroupID, int nRuleID)
            : base(nGroupID, nRuleID)
        {
            m_oMCMAIL = new MCWelcomeMail(m_nGroupID); 
        }

        public override bool SendMail(User user)
        {
            m_oMCMAIL.InitMCObj(m_nRuleID, user.m_oBasicData.m_sEmail, user.m_oBasicData.m_sFirstName, user.m_oBasicData.m_sLastName);
            return m_oMCMAIL.Send();
        }
    }
}
