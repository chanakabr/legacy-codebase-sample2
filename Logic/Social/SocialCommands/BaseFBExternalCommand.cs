using ApiObjects;
using Core.Social.Requests;
using Core.Social.Responses;
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
    public abstract class BaseFBExternalCommand
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected User m_oUser;
        protected eUserAction m_eUserAction;
        protected int m_nAssetID;
        protected int m_nGroupID;
        protected FacebookWrapper m_oFBWrapper;
        protected BaseSocialBL m_oSocialBL;
        protected eAssetType m_eAssetType;
        protected Dictionary<string, string> m_dExtraParams;

        public virtual SocialActionResponseStatus Execute(out string sFBObjectID, out string sFBActionID)
        {
            sFBObjectID = string.Empty;
            sFBActionID = string.Empty;
            string sDecryptedToken;

            eSocialActionPrivacy eActionPrivacy = m_oSocialBL.GetUserExternalActionShare(m_oUser.m_sSiteGUID, SocialPlatform.FACEBOOK, m_eUserAction);

            if (eActionPrivacy != eSocialActionPrivacy.ALLOW)
            {
                return SocialActionResponseStatus.NOT_ALLOWED;
            }

            if (!ValidateUserCredentials(out sDecryptedToken))
            {
                return SocialActionResponseStatus.INVALID_ACCESS_TOKEN;
            }

            m_oSocialBL.GetFBObjectID(m_nAssetID, m_eAssetType, ref sFBObjectID);
            if (string.IsNullOrEmpty(sFBObjectID))
            {
                try
                {
                    sFBObjectID = createFBObject();
                }
                catch { }

                if (string.IsNullOrEmpty(sFBObjectID))
                {
                    return SocialActionResponseStatus.EMPTY_FB_OBJECT_ID;
                }
            }

            eSocialPrivacy ePrivacy = m_oSocialBL.GetUserSocialPrivacy(int.Parse(m_oUser.m_sSiteGUID), SocialPlatform.FACEBOOK, m_eUserAction);

            string sJsonPrivacy = string.Empty;


            m_oFBWrapper.GetPrivacyGroupJSONString(m_oUser.m_sSiteGUID, Utils.GetValFromConfig("FB_LIST_NAME"), ePrivacy, ref sJsonPrivacy);
            if (!string.IsNullOrEmpty(sJsonPrivacy))
            {
                m_dExtraParams["privacy"] = sJsonPrivacy;
            }

            SocialActionResponseStatus response = DoUserAction(ref sDecryptedToken, ref sFBObjectID, out sFBActionID);

            return response;
        }

        public virtual ApiObjects.Response.Status ExecuteAction(SocialPrivacySettings privacySettings, out string FBObjectID, out string FBActionID)
        {
            FBObjectID = string.Empty;
            FBActionID = string.Empty;
            string sDecryptedToken;
            SocialNetwork fbNetwork = null;
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error ,ApiObjects.Response.eResponseStatus.Error.ToString());

            if (privacySettings.SocialNetworks != null && privacySettings.SocialNetworks.Count > 0)
            {
                fbNetwork = privacySettings.SocialNetworks.Where(x => x.Network == SocialPlatform.FACEBOOK).FirstOrDefault();
                // default value is dont_allow
                if (fbNetwork == null || fbNetwork.Privacy == eSocialActionPrivacy.DONT_ALLOW)
                    return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.SocialActionPrivacyDontAllow, ApiObjects.Response.eResponseStatus.SocialActionPrivacyDontAllow.ToString());
            }

            if (!ValidateUserCredentials(out sDecryptedToken))
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.InvalidAccessToken, ApiObjects.Response.eResponseStatus.InvalidAccessToken.ToString());
            }

            m_oSocialBL.GetFBObjectID(m_nAssetID, m_eAssetType, ref FBObjectID);
            if (string.IsNullOrEmpty(FBObjectID))
            {
                try
                {
                    FBObjectID = createFBObject();
                }
                catch { }

                if (string.IsNullOrEmpty(FBObjectID))
                {
                    return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.EmptyFacebookObjectId ,ApiObjects.Response.eResponseStatus.EmptyFacebookObjectId.ToString());;
                }
            }

            eSocialPrivacy ePrivacy = fbNetwork.SocialPrivacy;

            string sJsonPrivacy = string.Empty;


            m_oFBWrapper.GetPrivacyGroupJSONString(m_oUser.m_sSiteGUID, Utils.GetValFromConfig("FB_LIST_NAME"), ePrivacy, ref sJsonPrivacy);
            if (!string.IsNullOrEmpty(sJsonPrivacy))
            {
                m_dExtraParams["privacy"] = sJsonPrivacy;
            }

            SocialActionResponseStatus response = DoUserAction(ref sDecryptedToken, ref FBObjectID, out FBActionID);
            status = ConvertSocialActionResponseStatus(response);
            return status;
        }

        private ApiObjects.Response.Status ConvertSocialActionResponseStatus(SocialActionResponseStatus response)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());

            switch (response)
            {
                case SocialActionResponseStatus.OK:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                    break;               
                case SocialActionResponseStatus.UNKNOWN_ACTION:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.UnknownAction, ApiObjects.Response.eResponseStatus.UnknownAction.ToString());
                    break;
                case SocialActionResponseStatus.INVALID_ACCESS_TOKEN:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.InvalidAccessToken, ApiObjects.Response.eResponseStatus.InvalidAccessToken.ToString());
                    break;
                case SocialActionResponseStatus.INVALID_PLATFORM_REQUEST:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.InvalidPlatformRequest, ApiObjects.Response.eResponseStatus.InvalidPlatformRequest.ToString());
                    break;
                case SocialActionResponseStatus.MEDIA_DOESNT_EXISTS:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.AssetDoseNotExists, ApiObjects.Response.eResponseStatus.AssetDoseNotExists.ToString());
                    break;
                case SocialActionResponseStatus.MEDIA_ALREADY_LIKED:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.AssetAlreadyLiked, ApiObjects.Response.eResponseStatus.AssetAlreadyLiked.ToString());
                    break;
                case SocialActionResponseStatus.INVALID_PARAMETERS:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.InvalidParameters, ApiObjects.Response.eResponseStatus.InvalidParameters.ToString());
                    break;
                case SocialActionResponseStatus.USER_DOES_NOT_EXIST:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.UserDoesNotExist, ApiObjects.Response.eResponseStatus.UserDoesNotExist.ToString());
                    break;
                case SocialActionResponseStatus.NO_FB_ACTION:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.NoFacebookAction, ApiObjects.Response.eResponseStatus.NoFacebookAction.ToString());
                    break;
                case SocialActionResponseStatus.EMPTY_FB_OBJECT_ID:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.EmptyFacebookObjectId, ApiObjects.Response.eResponseStatus.EmptyFacebookObjectId.ToString());
                    break;
                case SocialActionResponseStatus.NOT_ALLOWED:
                    status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.NotAllowed, ApiObjects.Response.eResponseStatus.NotAllowed.ToString());
                    break;
            case SocialActionResponseStatus.UNKNOWN:
            case SocialActionResponseStatus.ERROR:
            case SocialActionResponseStatus.CONFIG_ERROR:
            default:
                    break;
            }
            return status;
        }

        protected abstract SocialActionResponseStatus DoUserAction(ref string sDecryptedToken, ref string sFBObjectID, out string sFBActionID);

        private bool ValidateUserCredentials(out string sDecryptedToken)
        {
            sDecryptedToken = string.Empty;
            bool bResult = false;
            if (!string.IsNullOrEmpty(m_oUser.m_oBasicData.m_sFacebookID) && !string.IsNullOrEmpty(m_oUser.m_oBasicData.m_sFacebookToken))
            {
                try
                {
                    string eToken = m_oUser.m_oBasicData.m_sFacebookToken;
                    string key = Utils.GetValFromConfig("FB_TOKEN_KEY");
                    sDecryptedToken = Utils.Decrypt(eToken, key);
                    bResult = true;
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Could not decrypt FB token. Token to decrypt={0}. ex={1}; stack={2}", m_oUser.m_oBasicData.m_sFacebookToken, ex.Message, ex.StackTrace), ex);
                }
            }

            return bResult;
        }

        protected string createFBObject()
        {
            string sObjectID = string.Empty;
            string sUrl = string.Empty;

            ApiObjects.KeyValuePair urlKvp = null;


            if (m_dExtraParams.ContainsKey("obj:url") && !string.IsNullOrEmpty(m_dExtraParams["obj:url"]))
            {
                urlKvp = new ApiObjects.KeyValuePair("obj:url", m_dExtraParams["obj:url"]);
            }

            if (urlKvp != null)
            {
                SocialObjectReponse objResponse = null;
                try
                {
                    FacebookObjectRequest objRequest = new FacebookObjectRequest()
                    {
                        m_eAssetType = m_eAssetType,
                        m_eSocialPlatform = SocialPlatform.FACEBOOK,
                        m_nAssetID = m_nAssetID,
                        m_eType = eRequestType.SET
                    };
                    objRequest.m_oKeyValue.Add(urlKvp);

                    objResponse = objRequest.GetResponse(m_nGroupID) as SocialObjectReponse;
                }
                catch
                {
                }
                if (objResponse != null)
                {
                    sObjectID = objResponse.sID;
                }

            }
            else
            {
                log.Error("Error - "+ string.Format("obj:url not passed in request. site_guid={0}; action={1}; asset_id={2}", m_oUser.m_sSiteGUID, m_eUserAction.ToString(), m_nAssetID));
            }
            return sObjectID;
        }
    }
}
