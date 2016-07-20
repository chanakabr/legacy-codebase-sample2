using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Web;
using KLogMonitor;

namespace SocialFeedHandler
{
    public class SocialFeeder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int m_nGroupID { get; set; }
        public int m_sSiteGuid { get; set; }

        public SocialFeeder(int nGroupId, int sSiteGuid)
        {
            m_nGroupID = nGroupId;
            m_sSiteGuid = sSiteGuid;
        }

        public bool UpdateFriendsFeed(string sDbActionID)
        {
            bool bResult = false;

            if (m_nGroupID == 0 || m_sSiteGuid == 0 || string.IsNullOrEmpty(sDbActionID))
            {
                throw new ArgumentNullException("UpdateFriendsFeed - Must provide a value for group ID, site guid, and db action id");
            }

            using (SocialReference.module socialRef = new SocialReference.module())
            {
                try
                {
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "UpdateFriendsFeed", "social", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("social_ws");
                    if (sWSURL.Length > 0)
                        socialRef.Url = sWSURL;

                    bResult = socialRef.UpdateFriendsActivityFeed(sWSUserName, sWSPass, m_sSiteGuid, sDbActionID);
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Error occurred while updating friends feed. groupId: {0}, Siteguid: {1} exception: {2}", m_nGroupID, m_sSiteGuid, ex.Message), ex);
                }
            }

            return bResult;
        }

        public void DeleteUserFeed()
        {
            if (m_nGroupID == 0 || m_sSiteGuid == 0)
            {
                throw new ArgumentNullException("DeleteUserFeed - Must provide a value for group ID and site guid");
            }

            using (SocialReference.module socialRef = new SocialReference.module())
            {
                try
                {
                    string sIP = "1.1.1.1";
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "DeleteUserFeed", "social", sIP, ref sWSUserName, ref sWSPass);
                    string sWSURL = TVinciShared.WS_Utils.GetTcmConfigValue("social_ws");
                    if (sWSURL.Length > 0)
                        socialRef.Url = sWSURL;

                    socialRef.DeleteFriendsFeed(sWSUserName, sWSPass, m_sSiteGuid);
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Error occurred while deleting user feed. groupId: {0}, Siteguid: {1} exception: {2}", m_nGroupID, m_sSiteGuid, ex.Message), ex);
                }
            }
        }
    }
}

namespace SocialFeedHandler.SocialReference
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);

            if (KLogMonitor.KLogger.AppType == KLogEnums.AppType.WCF)
            {
                if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(Constants.REQUEST_ID_KEY, OperationContext.Current.IncomingMessageProperties[KLogMonitor.Constants.REQUEST_ID_KEY].ToString());
                }
            }
            else
            {
                if (request.Headers != null &&
                request.Headers[Constants.REQUEST_ID_KEY] == null &&
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] != null)
                {
                    request.Headers.Add(Constants.REQUEST_ID_KEY, HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
                }
            }
            return request;
        }
    }
}