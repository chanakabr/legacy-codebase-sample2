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

        public SocialFeeder(int nGroupId, string sSiteGuid)
        {
            m_nGroupID = nGroupId;
            m_sSiteGuid = sSiteGuid;
        }

        public bool UpdateFriendsFeed(string sDbActionID)
        {
            bool bResult = false;

            if (m_nGroupID == 0 || string.IsNullOrEmpty(m_sSiteGuid) || string.IsNullOrEmpty(sDbActionID))
            {
                throw new ArgumentNullException("UpdateFriendsFeed - Must provide a valud for group ID, site guid, and db action id");
            }

            FacebookWrapper oFBWrapper = new FacebookWrapper(m_nGroupID);
            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID) as BaseSocialBL;
            

            List<string> lFriendsGuid = null;

            int nSiteGuid;
            if (int.TryParse(m_sSiteGuid, out nSiteGuid))
            {
                if (oFBWrapper.GetUserFriendsGuid(nSiteGuid, out lFriendsGuid))
                {
                    if(lFriendsGuid != null && lFriendsGuid.Count > 0)
                        bResult = oSocialBL.UpdateUserActivityFeed(lFriendsGuid, sDbActionID);
                }
                else
                {
                    Logger.Logger.Log("Info", "caught error when getting user friends guid. site guid={0}", "SocialFeedHandler");
                }
            }
            return bResult;
        }

        public void DeleteUserFeed()
        {
            if (m_nGroupID == 0 || string.IsNullOrEmpty(m_sSiteGuid))
            {
                throw new ArgumentNullException("DeleteUserFeed - Must provide a valud for group ID and site guid");
            }

            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID);

            List<string> lDocIDs;
            bool bHasNoErrors = true;

            int nNumOfDocs = 50;

            do
            {
                bHasNoErrors = oSocialBL.GetFeedIDsByActorID(m_sSiteGuid, nNumOfDocs, out lDocIDs);
                if (bHasNoErrors && lDocIDs != null && lDocIDs.Count > 0)
                {
                    foreach (string docID in lDocIDs)
                    {
                        bHasNoErrors &= oSocialBL.DeleteActivityFromUserFeed(docID);
                    }
                }

            } while (bHasNoErrors == true && lDocIDs != null && lDocIDs.Count > 0);

            if (!bHasNoErrors)
            {
                throw new Exception("Error occured during deletion of feed");
            }
        }
    }
}
