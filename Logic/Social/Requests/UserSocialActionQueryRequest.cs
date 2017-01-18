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
    public class UserSocialActionQueryRequest : SocialBaseRequestWrapper
    {
        public UserSocialActionQueryRequest()
        {
            m_eUserActions = eUserAction.UNKNOWN;
            m_sSiteGuid = string.Empty;
            m_lAssetIDs = new List<int>();
            m_nStartIndex = 0;
            m_nNumOfRecords = 0;
            m_eSocialPlatform = SocialPlatform.UNKNOWN;
            m_eAssetType = eAssetType.UNKNOWN;
            m_sFunctionName = "GetUserAction";
        }


        public eUserAction m_eUserActions { get; set; }

        public string m_sSiteGuid { get; set; }
        public List<int> m_lAssetIDs { get; set; }
        public int m_nStartIndex { get; set; }
        public int m_nNumOfRecords { get; set; }
        public eAssetType m_eAssetType { get; set; }
        public override string m_sFunctionName { get; set; }
        public List<eUserAction> UserActions { get; set; }

        public override BaseSocialResponse GetResponse(int nGroupID)
        {
            m_nGroupID = nGroupID;
            SocialActionQueryResponse response = new SocialActionQueryResponse(STATUS_FAIL);

            if (string.IsNullOrEmpty(m_sSiteGuid) || m_lAssetIDs == null || m_lAssetIDs.Count == 0)
            {
                return response;
            }

            response.m_nStatus = STATUS_OK;

            int nTopNum = (m_nNumOfRecords > 0) ? m_nStartIndex + m_nNumOfRecords : m_nNumOfRecords;

            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID) as BaseSocialBL;
            
            List<int> lActions = UserActions != null ? UserActions.Select(a => (int)a).ToList() : null;
            if (lActions == null || lActions.Count == 0)
            {
                lActions = ApiObjects.SocialObjects.GetAllSelectedItems<int, eUserAction>(m_eUserActions).ToList();
            }
            List<SocialActivityDoc> lSocialActions = oSocialBL.GetUserSocialAction(m_sSiteGuid, m_eSocialPlatform, m_eAssetType, lActions, m_lAssetIDs);
            response.m_lUserActionObj = Utils.GetTopRecords<SocialActivityDoc>(m_nNumOfRecords, m_nStartIndex, lSocialActions);            
            response.TotalCount = lSocialActions != null ? lSocialActions.Count : 0;
            return response;
        }


    }

}
