using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Social.Responses;
using ApiObjects;
using ApiObjects.Social;

namespace Core.Social.Requests
{
    public class BaseSocialPrivacyRequest : SocialBaseRequestWrapper
    {
        public BaseSocialPrivacyRequest(int nSiteGuid, int nGroupID, SocialPlatform eSocialPlatform)
        {
            m_nSiteGuid = nSiteGuid;
            m_nGroupID = nGroupID;
            m_eSocialPlatform = eSocialPlatform;
            m_sFunctionName = "PrivacyRequest";
            m_nGroupID = 0;
        }

        public int m_nSiteGuid { get; set; }

        public eRequestType m_eType { get; set; }

        public eSocialPrivacy m_ePrivacy { get; set; }

        public eUserAction m_eUserAction { get; set; }

        public override string m_sFunctionName { get; set; }

        public override BaseSocialResponse GetResponse(int nGroupID)
        {
            m_nGroupID = nGroupID;
            BaseSocialPrivacyReponse response = new BaseSocialPrivacyReponse(STATUS_FAIL);
            response.m_ePrivacy = eSocialPrivacy.UNKNOWN;
            BaseSocialBL oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID) as BaseSocialBL;

            if (m_eType == eRequestType.GET)
            {
                eSocialPrivacy ePrivacy = oSocialBL.GetUserSocialPrivacy(m_nSiteGuid, m_eSocialPlatform, m_eUserAction);
                if (ePrivacy != eSocialPrivacy.UNKNOWN)
                {
                    response.m_nStatus = STATUS_OK;
                    response.m_ePrivacy = ePrivacy;
                }

            }
            else if(m_eType == eRequestType.SET)
            {
                if (m_ePrivacy != eSocialPrivacy.UNKNOWN)
                {
                    bool bRes = oSocialBL.SetUserSocialPrivacy(m_nSiteGuid, m_eSocialPlatform, m_eUserAction, m_ePrivacy);
                    if (bRes)
                    {
                        response.m_nStatus = STATUS_OK;
                        response.m_ePrivacy = m_ePrivacy;
                    }
                }
            }

            return response;
        }
    }
}
