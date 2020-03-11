using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Users;
using ApiObjects.Social;

namespace Core.Social.SocialCommands
{
    public class FBLikeExternalCommand : BaseFBExternalCommand
    {
        public FBLikeExternalCommand(User oUser, int nGroupID, eAssetType eAssetType, int nAssetID, List<ApiObjects.KeyValuePair> lExtraParams)
        {
            m_oUser = oUser;
            m_eUserAction = eUserAction.LIKE;
            m_eAssetType = eAssetType;
            m_nAssetID = nAssetID;
            m_nGroupID = nGroupID;
            m_oFBWrapper = new FacebookWrapper(nGroupID);
            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(nGroupID);
            m_dExtraParams = Utils.KvpListToDictionary(ref lExtraParams);
        }

        protected override SocialActionResponseStatus DoUserAction(ref string sDecryptedToken, ref string sFBObjectID, out string sFBActionID)
        {
            SocialActionResponseStatus eResponse = SocialActionResponseStatus.UNKNOWN;

            sFBActionID = string.Empty;

            m_dExtraParams["object"] = sFBObjectID;

            int nStatus = 0;
            string sRetVal;

            string sExtraParams = string.Empty;

            foreach (string key in m_dExtraParams.Keys)
            {
                Utils.AddParameter(key, m_dExtraParams[key], ref sExtraParams);
            }

            sRetVal = Utils.SendPostHttpReq(string.Format("{0}/og.likes?access_token={1}", FBUtils.FB_GRAPH_URI_ME_PREFIX, sDecryptedToken), ref nStatus, string.Empty, string.Empty, sExtraParams);

            if (nStatus == FBUtils.STATUS_OK)
            {
                FBActionResponse fbResponse = Utils.Deserialize<FBActionResponse>(sRetVal);

                if (fbResponse != null)
                {
                    sFBActionID = fbResponse.id;
                    eResponse = SocialActionResponseStatus.OK;
                }
            }
            else
            {
                eResponse = FBUtils.GetFacebookError(sRetVal);
            }

            return eResponse;
        }
    }
}
