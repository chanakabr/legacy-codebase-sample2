using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Core.Users;
using ApiObjects.Social;

namespace Core.Social.SocialCommands
{
    public class FBShareExternalCommand : BaseFBExternalCommand
    {
        public FBShareExternalCommand(User oUser, int nGroupID, eAssetType eAssetType, int nAssetID, List<ApiObjects.KeyValuePair> lExtraParams)
        {
            m_oUser = oUser;
            m_eUserAction = eUserAction.SHARE;
            m_eAssetType = eAssetType;
            m_nAssetID = nAssetID;
            m_nGroupID = nGroupID;
            m_oFBWrapper = new FacebookWrapper(nGroupID);
            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(nGroupID);
            m_dExtraParams = Utils.KvpListToDictionary(ref lExtraParams);
        }

        protected override SocialActionResponseStatus DoUserAction(ref string sDecryptedToken, ref string sFBObjectID, out string sFBActionID)
        {
            SocialActionResponseStatus eRes = SocialActionResponseStatus.UNKNOWN;
            sFBActionID = string.Empty;

            if (!FBUtils.CanUserShare(m_oUser.m_oBasicData.m_sFacebookID, sDecryptedToken))
            {
                eRes = SocialActionResponseStatus.INVALID_ACCESS_TOKEN;
                return eRes;
            }

            int nStatus = 0;
            string link, sParams = string.Empty;

            m_dExtraParams.TryGetValue("link", out link);
            if (string.IsNullOrEmpty(link))
            {
                m_oSocialBL.GetMediaLinkPostParameters(m_nAssetID, ref m_dExtraParams);
            }

            var url = string.Format("{0}/feed?access_token={1}", FBUtils.FB_GRAPH_URI_ME_PREFIX, sDecryptedToken);

            foreach (string key in m_dExtraParams.Keys)
            {
                Utils.AddParameter(key, m_dExtraParams[key], ref sParams);
            }

            string sRetVal = Utils.SendPostHttpReq(url, ref nStatus, string.Empty, string.Empty, sParams);

            FBActionResponse fbResponse = Utils.Deserialize<FBActionResponse>(sRetVal);

            if (nStatus == FBUtils.STATUS_OK)
            {
                sFBActionID = fbResponse.id;
                eRes = SocialActionResponseStatus.OK;
            }
            else
            {
                eRes = SocialActionResponseStatus.ERROR;

                if (!string.IsNullOrEmpty(sRetVal))
                {
                    int nErrorCode = int.Parse(fbResponse.error.code);

                    if (nErrorCode == 190 || nErrorCode == FBUtils.STATUS_OK)
                    {
                        eRes = SocialActionResponseStatus.INVALID_ACCESS_TOKEN;
                    }
                }
            }

            return eRes;
        }
    }
}
