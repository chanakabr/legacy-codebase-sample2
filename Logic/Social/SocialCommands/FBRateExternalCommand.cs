using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using ApiObjects.Social;

namespace Core.Social.SocialCommands
{
    public class FBRateExternalCommand : BaseFBExternalCommand
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public FBRateExternalCommand(User oUser, int nGroupID, eAssetType eAssetType, int nAssetID, List<ApiObjects.KeyValuePair> lExtraParams)
        {
            m_oUser = oUser;
            m_eUserAction = eUserAction.RATES;
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
            int nRateValue;

            if (!m_dExtraParams.ContainsKey("rating:value") || !int.TryParse(m_dExtraParams["rating:value"], out nRateValue))
            {
                eRes = SocialActionResponseStatus.INVALID_PARAMETERS;
                return eRes;
            }

            double nRatingScale = 5.0;

            double nRatingNormalized = (nRateValue - 1.0) / (nRatingScale - 1.0);

            FacebookManager fbManager = FacebookManager.GetInstance;

            FacebookConfig fbConfig = null;
            if (fbManager != null && (fbConfig = fbManager.GetFacebookConfigInstance(m_nGroupID)) != null)
            {

                string sObjectType = FBUtils.GetFBObjectType(fbConfig.AppSecret, sFBObjectID);

                if (sObjectType == "video")
                    sObjectType = "other";


                string sRetVal;
                string sParams = string.Empty;
                int nStatus = -1;

                Utils.AddParameter(sObjectType, sFBObjectID, ref sParams);
                Utils.AddParameter("rating:value", nRateValue.ToString(), ref sParams);
                Utils.AddParameter("rating:scale", nRatingScale.ToString(), ref sParams);
                Utils.AddParameter("rating:normalized_value", nRatingNormalized.ToString(), ref sParams);

                #region for future use: can add review
                //if (!string.IsNullOrEmpty(sReviewText))
                //{
                //    Utils.AddParameter("review_text", sReviewText, ref sParams);
                //}
                #endregion

                sRetVal = Utils.SendPostHttpReq(string.Format("{0}/video.rates?access_token={1}", FBUtils.FB_GRAPH_URI_ME_PREFIX, sDecryptedToken), ref nStatus, string.Empty, string.Empty, sParams);

                if (nStatus == FBUtils.STATUS_OK)
                {
                    FBActionResponse fbResponse = Utils.Deserialize<FBActionResponse>(sRetVal);

                    if (fbResponse != null)
                    {
                        sFBActionID = fbResponse.id;
                        eRes = SocialActionResponseStatus.OK;
                    }
                }
                else
                {
                    eRes = FBUtils.GetFacebookError(sRetVal);
                }
            }
            else
            {
                eRes = SocialActionResponseStatus.CONFIG_ERROR;
                log.Error("Error - could not load FB configuration manager");
            }

            return eRes;
        }
    }
}
