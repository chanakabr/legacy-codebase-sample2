using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Social.Responses;
using System.Data;
using TVinciShared;
using ApiObjects;

namespace Core.Social.Requests
{
    public class GetFriendsActionsRequest : UserSocialActionQueryRequest
    {
        public GetFriendsActionsRequest()
        {
            m_sFunctionName = "GetFriendsActions";
        }

        public override BaseSocialResponse GetResponse(int nGroupID)
        {
            List<SocialActivityDoc> lUserActions = new List<SocialActivityDoc>();
            m_nGroupID = nGroupID;
            SocialActionQueryResponse response = new SocialActionQueryResponse(STATUS_FAIL);
            FacebookWrapper oFBWrapper = new FacebookWrapper(m_nGroupID);
            
            if (string.IsNullOrEmpty(m_sSiteGuid))
            {
                return response;
            }

            List<int> lActions = UserActions != null ? UserActions.Select(a => (int)a).ToList() : null;
            if (lActions == null || lActions.Count == 0)
            {
                lActions = ApiObjects.SocialObjects.GetAllSelectedItems<int, eUserAction>(m_eUserActions).ToList();
            }
            int nTopNum = (m_nNumOfRecords > 0) ? m_nStartIndex + m_nNumOfRecords : m_nNumOfRecords;

            if (m_lAssetIDs == null || m_lAssetIDs.Count == 0 || m_lAssetIDs.Contains(0) )
            {
                BaseSocialBL oBL = BaseSocialBL.GetBaseSocialImpl(nGroupID);
                bool bResult = oBL.GetUserActivityFeed(m_sSiteGuid, 0, 0 , string.Empty, out lUserActions);

                if (bResult)
                {
                    response.m_nStatus = STATUS_OK;

                    lUserActions = lUserActions.Where(x => x.SocialPlatform == (int)m_eSocialPlatform && (x.ActivityObject.AssetType == m_eAssetType || m_eAssetType == eAssetType.UNKNOWN)
                        && lActions.Contains(x.ActivityVerb.ActionType)).ToList();

                    response.TotalCount = lUserActions.Count;
                    response.m_lUserActionObj = Utils.GetTopRecords<SocialActivityDoc>(m_nNumOfRecords, m_nStartIndex, lUserActions);
                }
                else
                {
                    response.m_nStatus = STATUS_FAIL;
                }
            }
            else
            {
                List<string> lFriends;

                if (oFBWrapper.GetUserFriendsGuid(int.Parse(m_sSiteGuid), out lFriends))
                {
                    if (lFriends != null && lFriends.Count > 0)
                    {
                        BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
                        List<SocialActivityDoc> lUserActions2 = oSocialBL.GetFriendsSocialActions(lFriends, m_eSocialPlatform, m_eAssetType, lActions, m_lAssetIDs, m_sSiteGuid);
                        lUserActions2 = lUserActions2.Where(sa => sa.IsActive).ToList();

                        response.TotalCount = lUserActions2.Count;
                        response.m_lUserActionObj = Utils.GetTopRecords<SocialActivityDoc>(m_nNumOfRecords, m_nStartIndex, lUserActions2);
                    }

                    response.m_nStatus = STATUS_OK;
                }
                else
                {
                    response.m_nStatus = STATUS_FAIL;
                }
            }
            return response;
        }
    }
}
