using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ConditionalAccess
{
    class ProjectorConditionalAccess : TvinciConditionalAccess
    {

         public ProjectorConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

         public ProjectorConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }
        
        protected override string GetLicensedLink(string sBasicLink, string sUserIP, string sRefferer)
        {
            if (sBasicLink.Contains("cdnetworks"))
            {
                string retVal = string.Empty;
                CDNetworksVault.MediaVault m = new CDNetworksVault.MediaVault("tv117ci", "guest", 300);
                Uri u = new Uri(sBasicLink);

                Uri fileUrl = new Uri(sBasicLink);
                string[] sFileSegments = fileUrl.Segments;
                string sPathWithoutFile = "";
                for (int i = 0; i < sFileSegments.Length - 1; i++)
                {
                    sPathWithoutFile += sFileSegments[i];
                }
                string sFile = sFileSegments[sFileSegments.Length - 1];
                string keyUrlStr = m.GetURL(fileUrl.Scheme + "://" + fileUrl.Host + sPathWithoutFile + sFile);
                Uri keyUrl = new Uri(keyUrlStr);
                //string keyStr = HttpUtility.ParseQueryString(keyUrl.Query).Get("key");
                //if (!string.IsNullOrEmpty(keyStr))
                //{
                //    retVal = sBasicLink.Replace("flashstream", string.Format("flashstream?key={0}", keyStr));
                //}
                retVal = keyUrl.ToString();
                return retVal;
            }
            else
            {
                return sBasicLink;
            }
            
        }

        protected override string GetErrorLicensedLink(string sBasicLink)
        {
            return "";
        }
    }
}
