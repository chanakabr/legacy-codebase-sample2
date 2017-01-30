using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class AddDeviceMailRequest : MailRequestObj
    {
        public string m_sToken;
        public string m_sMasterUsername;
        public string m_sNewDeviceUdid;
        public string m_sNewDeviceName;


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

            MCGlobalMergeVars masterUsernameMergeVar = new MCGlobalMergeVars();
            masterUsernameMergeVar.name = "MASTERUSERNAME";
            masterUsernameMergeVar.content = this.m_sMasterUsername;
            retVal.Add(masterUsernameMergeVar);

            MCGlobalMergeVars newUsernameMergeVar = new MCGlobalMergeVars();
            newUsernameMergeVar.name = "DEVICEUDID";
            newUsernameMergeVar.content = this.m_sNewDeviceUdid;
            retVal.Add(newUsernameMergeVar);

            MCGlobalMergeVars newUserFirstNameMergeVar = new MCGlobalMergeVars();
            newUserFirstNameMergeVar.name = "DEVICENAME";
            newUserFirstNameMergeVar.content = this.m_sNewDeviceName;
            retVal.Add(newUserFirstNameMergeVar);

            return retVal;
        }
    }
}
