using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ApiObjects
{
    public class SendToFriendMailRequest : MailRequestObj
    {

        public string m_sContentName;
        public string m_sLink;
        public string m_sMediaID;
        public string m_sMediaType;
        

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            MCGlobalMergeVars senderMergeVar = new MCGlobalMergeVars();
            senderMergeVar.name = "SENDERNAME";
            senderMergeVar.content = this.m_sSenderName;
            retVal.Add(senderMergeVar);
            MCGlobalMergeVars mediaNameMergeVar = new MCGlobalMergeVars();
            mediaNameMergeVar.name = "MEDIANAME";
            mediaNameMergeVar.content = this.m_sContentName;
            retVal.Add(mediaNameMergeVar);
            MCGlobalMergeVars mediaTypeMergeVar = new MCGlobalMergeVars();
            mediaTypeMergeVar.name = "MEDIATYPE";
            mediaTypeMergeVar.content = this.m_sMediaType;
            mediaTypeMergeVar.content = mediaTypeMergeVar.content.Replace("Test", "Movie");
            retVal.Add(mediaTypeMergeVar);
            MCGlobalMergeVars mediaIDMergeVar = new MCGlobalMergeVars();
            mediaIDMergeVar.name = "MEDIAID";
            mediaIDMergeVar.content = this.m_sMediaID;
            retVal.Add(mediaIDMergeVar);
            MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
            nameMergeVar.name = "FIRSTNAME";
            nameMergeVar.content = this.m_sFirstName;
            retVal.Add(nameMergeVar);
            MCGlobalMergeVars contentMergeVar = new MCGlobalMergeVars();
            contentMergeVar.name = "CONTENT";
            contentMergeVar.content = HttpUtility.UrlPathEncode(this.m_sContentName);
            retVal.Add(contentMergeVar);
            MCGlobalMergeVars linkMergeVar = new MCGlobalMergeVars();
            linkMergeVar.name = "LINK";
            linkMergeVar.content = this.m_sLink;
            retVal.Add(linkMergeVar);
            return retVal;
        }
    }
}
