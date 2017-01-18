using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Core.Social.Responses;
using ApiObjects;
using ApiObjects.Social;

namespace Core.Social.Requests
{
    public class FacebookObjectRequest : SocialBaseRequestWrapper
    {
        public FacebookObjectRequest()
        {
            m_eSocialPlatform = SocialPlatform.UNKNOWN;
            m_nAssetID = 0;
            m_eType = eRequestType.GET;
            m_sFunctionName = "FBObjectRequest";
            m_eAssetType = eAssetType.UNKNOWN;
            m_nGroupID = 0;
            this.m_oKeyValue = new List<KeyValuePair>();
        }

        public FacebookObjectRequest(int nGroupID, int nAssetID, eAssetType assetType, eRequestType eType)
        {
            m_eSocialPlatform = SocialPlatform.FACEBOOK;
            m_nAssetID = nAssetID;
            m_eType = eType;
            m_sFunctionName = "FBObjectRequest";
            m_eAssetType = assetType;
            m_nGroupID = nGroupID;
            this.m_oKeyValue = new List<KeyValuePair>();
        }

        public eAssetType m_eAssetType { get; set; }
        public eRequestType m_eType { get; set; }
        public int m_nAssetID { get; set; }
        public override string m_sFunctionName { get; set; }

        protected BaseSocialBL m_oSocialBL;
        protected FacebookWrapper m_oFBWrapper;

        public override BaseSocialResponse GetResponse(int nGroupID)
        {
            m_nGroupID = nGroupID;
            SocialObjectReponse oRes = new SocialObjectReponse(STATUS_FAIL);
            oRes.sID = string.Empty;

            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(nGroupID) as BaseSocialBL;
            m_oFBWrapper = new FacebookWrapper(nGroupID);

            string sFBObjectID = string.Empty;

            m_oSocialBL.GetFBObjectID(m_nAssetID, m_eAssetType, ref sFBObjectID);

            switch (m_eType)
            {
                case eRequestType.GET:
                    oRes.m_nStatus = STATUS_OK;
                    oRes.sID = sFBObjectID;
                    break;
                case eRequestType.SET:
                    if (string.IsNullOrEmpty(sFBObjectID))
                    {
                        oRes = CreateObject();
                        if (oRes.m_nStatus == STATUS_OK)
                            m_oSocialBL.SetAssetFBObjectID(m_nAssetID, m_eAssetType, oRes.sID);
                    }
                    else
                    {
                        oRes.m_nStatus = STATUS_OK;
                        oRes.sID = sFBObjectID;
                    }
                    break;
                //case eRequestType.DELETE:
                //    if (!string.IsNullOrEmpty(sFBObjectID))
                //    {
                //        oRes = DeleteObject(sFBObjectID);
                //    }
                //    break;
            }

            return oRes;
        }
        private SocialObjectReponse DeleteObject(string sFBObjectID)
        {
            SocialObjectReponse sRes = new SocialObjectReponse(STATUS_FAIL);

            string sRetVal = string.Empty;

            if (m_oFBWrapper.DeleteFBObject(sFBObjectID))
            {
                sRes.m_nStatus = STATUS_OK;
                sRes.sID = sFBObjectID;
            }

            return sRes;
        }

        private SocialObjectReponse CreateObject()
        {
            SocialObjectReponse sRes = new SocialObjectReponse(STATUS_FAIL);

            string sUrl = string.Empty;

            if (m_oKeyValue != null && m_oKeyValue.Count > 0)
            {
                sUrl = m_oKeyValue.Where(kvp => kvp.key == "obj:url").Select(kvp => kvp.value).First();
            }

            FacebookManager fbManager = FacebookManager.GetInstance;
            if (fbManager != null)
            {
                FacebookConfig fbConfig = fbManager.GetFacebookConfigInstance(m_nGroupID);
                string sCreateObjectResponse;

                switch (m_eAssetType)
                {
                    case eAssetType.MEDIA:
                        sCreateObjectResponse = m_oFBWrapper.CreateFBMediaObject(m_nAssetID, sUrl);
                        break;
                    case eAssetType.PROGRAM:
                        sCreateObjectResponse = m_oFBWrapper.CreateFBProgramObject(m_nAssetID, sUrl);
                        break;
                    default:
                        sCreateObjectResponse = string.Empty;
                        break;
                }

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                FBObjectReponse oCreateObjReponse = serializer.Deserialize<FBObjectReponse>(sCreateObjectResponse);

                if (oCreateObjReponse != null && !string.IsNullOrEmpty(oCreateObjReponse.id))
                {
                    sRes.m_nStatus = STATUS_OK;
                    sRes.sID = oCreateObjReponse.id;
                }
            }

            return sRes;
        }
    }
}
