using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class SendAdminTokenRequest : MailRequestObj
    {

        public string m_sToken;
        public string m_sIP;
        public string m_sDuration;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            MCGlobalMergeVars senderMergeVar = new MCGlobalMergeVars();
            senderMergeVar.name = "TOKEN";
            senderMergeVar.content = this.m_sToken;
            retVal.Add(senderMergeVar);
            MCGlobalMergeVars mediaNameMergeVar = new MCGlobalMergeVars();
            mediaNameMergeVar.name = "IP";
            mediaNameMergeVar.content = this.m_sIP;
            retVal.Add(mediaNameMergeVar);
            MCGlobalMergeVars mediaTypeMergeVar = new MCGlobalMergeVars();
            mediaTypeMergeVar.name = "DURATION";
            mediaTypeMergeVar.content = this.m_sDuration;
            mediaTypeMergeVar.content = mediaTypeMergeVar.content.Replace("Test", "Movie");
            retVal.Add(mediaTypeMergeVar);
            return retVal;
        }
    }
}
