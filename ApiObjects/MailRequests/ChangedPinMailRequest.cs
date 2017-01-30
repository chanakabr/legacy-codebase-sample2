using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ChangedPinMailRequest : MailRequestObj
    {
        public string m_sToken;
        public string m_sSiteGuid;
        public string m_sRuleName;
       

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();  

            MCGlobalMergeVars ruleNameVar = new MCGlobalMergeVars();
            ruleNameVar.name = "RULENAME";
            ruleNameVar.content = this.m_sRuleName;
            retVal.Add(ruleNameVar);

            MCGlobalMergeVars tokenVar = new MCGlobalMergeVars();
            tokenVar.name = "TOKEN";
            tokenVar.content = this.m_sToken;
            retVal.Add(tokenVar);

            MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
            nameMergeVar.name = "FIRSTNAME";
            nameMergeVar.content = this.m_sFirstName;
            retVal.Add(nameMergeVar);


            return retVal;
        }
    }
}
