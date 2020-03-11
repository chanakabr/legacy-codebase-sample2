using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace Core.Social
{
    public interface ISocialBL
    {
        bool InsertUserSocialAction(string sSiteGuid, string sDeviceUDID, int nAssetID, eAssetType assetType, eUserAction userAction, SocialPlatform eSocialPlatform, string sFBObjectID, string sFBActionID, int nRateValue = 0);
        List<SocialActionDoc> GetUserSocialAction(string sSiteGuid, int nSocialPlatform, eAssetType assetType, int nSocialAction = 0, int nMediaID = 0);
        List<SocialActionDoc> GetUsersSocialActions(int nNumOfRecords, List<string> lSiteGuidList, SocialPlatform eSocialPlatform, eUserAction eSocialAction, eAssetType assetType);
        List<SocialActionDoc> GetUsersSocialActionsOnAsset(int nNumOfRecords, List<string> lSiteGuidList, SocialPlatform eSocialPlatform, eUserAction eSocialAction, int nAssetID, eAssetType assetType);
    }
}
