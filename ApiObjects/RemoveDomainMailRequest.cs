using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class RemoveDomianMailRequest : MailRequestObj
    {        
        public string userName;
        public string useEmail;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();    
            
            MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
            nameMergeVar.name = "FIRSTNAME";
            nameMergeVar.content = this.m_sFirstName;
            retVal.Add(nameMergeVar);

            MCGlobalMergeVars lastNameMergeVar = new MCGlobalMergeVars();
            lastNameMergeVar.name = "LASTNAME";
            lastNameMergeVar.content = this.m_sLastName;
            retVal.Add(lastNameMergeVar);
         
            MCGlobalMergeVars userNameMergeVar = new MCGlobalMergeVars();
            userNameMergeVar.name = "USERNAME";
            userNameMergeVar.content = this.userName;
            retVal.Add(userNameMergeVar);

            MCGlobalMergeVars userEmailMergeVar = new MCGlobalMergeVars();
            userEmailMergeVar.name = "USEREMAIL";
            userEmailMergeVar.content = this.useEmail;
            retVal.Add(userEmailMergeVar);         

            return retVal;
        }
    } 
}
