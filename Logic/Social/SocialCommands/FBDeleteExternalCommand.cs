using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using ApiObjects.Social;

namespace Core.Social.SocialCommands
{
    public class FBDeleteExternalCommand : BaseFBExternalCommand
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public FBDeleteExternalCommand(User oUser, int nGroupID, eUserAction eAction, eAssetType eAssetType, int nAssetID, List<ApiObjects.KeyValuePair> lExtraParams)
        {
            m_oUser = oUser;
            m_eUserAction = eAction;
            m_eAssetType = eAssetType;
            m_nAssetID = nAssetID;
            m_nGroupID = nGroupID;
            m_dExtraParams = Utils.KvpListToDictionary(ref lExtraParams);
            m_oFBWrapper = new FacebookWrapper(nGroupID);
            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID);
        }


        protected override SocialActionResponseStatus DoUserAction(ref string sDecryptedToken, ref string sFBObjectID, out string sFBActionID)
        {
            sFBActionID = string.Empty;

            SocialActivityDoc oDoc = null;

            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID);
            oSocialBL.GetUserSocialAction(m_oUser.m_sSiteGUID, SocialPlatform.FACEBOOK, m_eAssetType, m_eUserAction, m_nAssetID, out oDoc);

            if (oDoc == null)
            {
                log.Error("Error - " + string.Format("Could not delete user {0} action as action was not found in DB. site_guid={1}; assetID={2}", m_eUserAction.ToString(), m_oUser.m_sSiteGUID, m_nAssetID));
                return SocialActionResponseStatus.ERROR;
            }

            sFBActionID = oDoc.ActivityVerb.SocialActionID;

            if (string.IsNullOrEmpty(oDoc.ActivityVerb.SocialActionID))
            {
                log.Error("Error - " + string.Format("Could not delete user {0} action as previous action does not have an external action id. site_guid={1}; assetID={2}", m_eUserAction.ToString(), m_oUser.m_sSiteGUID, m_nAssetID));
                return SocialActionResponseStatus.NO_FB_ACTION;
            }

            SocialActionResponseStatus eResponse = FBUtils.DeleteUserActionOnObject(sDecryptedToken, sFBActionID);

            return eResponse;
        }
    }
}
