using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class WelcomeMailRequest : MailRequestObj
    {
        public string m_sToken;
        public string m_sUsername;
        public string m_sPassword;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            MCGlobalMergeVars tokenMergeVar = new MCGlobalMergeVars();
            tokenMergeVar.name = "TOKEN";
            tokenMergeVar.content = this.m_sToken;
            retVal.Add(tokenMergeVar);
            MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
            nameMergeVar.name = "FIRSTNAME";
            nameMergeVar.content = this.m_sFirstName;
            retVal.Add(nameMergeVar);
            MCGlobalMergeVars usernameMergeVar = new MCGlobalMergeVars();
            usernameMergeVar.name = "USERNAME";
            usernameMergeVar.content = this.m_sUsername;
            retVal.Add(usernameMergeVar);
            MCGlobalMergeVars passMergeVar = new MCGlobalMergeVars();
            passMergeVar.name = "PASSWORD";
            passMergeVar.content = this.m_sPassword;
            retVal.Add(passMergeVar);
            return retVal;
        }
    }
}
