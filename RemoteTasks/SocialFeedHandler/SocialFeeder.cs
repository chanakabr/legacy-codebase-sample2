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

            try
            {
                bResult = Core.Social.Module.UpdateFriendsActivityFeed(m_nGroupID, m_sSiteGuid, sDbActionID);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Error occurred while updating friends feed. groupId: {0}, Siteguid: {1} exception: {2}", m_nGroupID, m_sSiteGuid, ex.Message), ex);
            }

            return bResult;
        }

        public void DeleteUserFeed()
        {
            if (m_nGroupID == 0 || m_sSiteGuid == 0)
            {
                throw new ArgumentNullException("DeleteUserFeed - Must provide a value for group ID and site guid");
            }

            try
            {
                Core.Social.Module.DeleteFriendsFeed(m_nGroupID, m_sSiteGuid);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Error occurred while deleting user feed. groupId: {0}, Siteguid: {1} exception: {2}", m_nGroupID, m_sSiteGuid, ex.Message), ex);
            }
        }
    }
}