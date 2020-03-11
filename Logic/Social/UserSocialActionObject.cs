using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace Core.Social
{
    public class UserSocialActionObject
    {
        public string m_sSiteGuid;
        public eUserAction m_eSocialAction;
        public SocialPlatform m_eSocialPlatform;
        public int nMediaID;
        public int nProgramID;
        public eAssetType assetType;
        public DateTime m_dActionDate;
        public int nRateValue = 0;

        public UserSocialActionObject()
        {
            m_sSiteGuid = string.Empty;
            m_eSocialAction = eUserAction.UNKNOWN;
            m_eSocialPlatform = SocialPlatform.UNKNOWN;
            nMediaID = 0;
            nProgramID = 0;
            m_dActionDate = new DateTime(2000, 1, 1);
            assetType = eAssetType.UNKNOWN;
            nRateValue = 0;

        }
    }

    
}
