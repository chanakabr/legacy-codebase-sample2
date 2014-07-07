using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Social;
using SocialBL;

namespace SocialFeedHandler
{
    public class SocialFeeder
    {
        public int m_nGroupID { get; set; }
        public string m_sSiteGuid { get; set; }
        public string m_sDbActionID { get; set; }

        public SocialFeeder(int nGroupId, string sSiteGuid, string sDbActionID)
        {
            m_nGroupID = nGroupId;
            m_sSiteGuid = sSiteGuid;
            m_sDbActionID = sDbActionID;
        }

        public bool UpdateFriendsFeed()
        {
            bool bResult = false;

            if (m_nGroupID == 0 || string.IsNullOrEmpty(m_sSiteGuid) || string.IsNullOrEmpty(m_sDbActionID))
            {
                throw new ArgumentNullException("Must provide a valud for group ID, site guid, and db action id");
            }

            FacebookWrapper oFBWrapper = new FacebookWrapper(m_nGroupID);
            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID) as BaseSocialBL;
            

            List<string> lFriendsGuid = null;

            int nSiteGuid;
            if (int.TryParse(m_sSiteGuid, out nSiteGuid))
            {
                int nNumOfFriends = oFBWrapper.GetUserFriendsGuid(nSiteGuid, out lFriendsGuid);

                if (nNumOfFriends > 0)
                {
                    bResult = oSocialBL.AddActivityToUserFeed(lFriendsGuid, m_sDbActionID);
                }
                else
                {
                    Logger.Logger.Log("Info", "user with site guid {0} has no friends. action with id {1} not published to feed", "SocialFeedHandler");
                }
            }
            return bResult;
        }
    }
}
