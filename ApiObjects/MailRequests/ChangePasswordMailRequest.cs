using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ChangePasswordMailRequest : MailRequestObj
    {
        public string m_sToken;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();

            switch (this.m_eMailType)
            {
                case eMailTemplateType.ChangePassword:

                    MCGlobalMergeVars tokenVar = new MCGlobalMergeVars();
                    tokenVar.name = "TOKEN";
                    tokenVar.content = this.m_sToken;
                    retVal.Add(tokenVar);
                    MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
                    nameMergeVar.name = "FIRSTNAME";
                    nameMergeVar.content = this.m_sFirstName;
                    retVal.Add(nameMergeVar);

                    break;
            }
            
            return retVal;
        }
    }
}
