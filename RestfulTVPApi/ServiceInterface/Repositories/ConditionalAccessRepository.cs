using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace RestfulTVPApi.ServiceInterface
{
    public class ConditionalAccessRepository : IConditionalAccessRepository
    {
        //Ofir - Should SiteGuid be a param?
        public bool ActivateCampaign(InitializationObject initObj, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                   CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            bool res = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaign", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                CampaignActionInfo actionInfo = new CampaignActionInfo()
                {
                    m_siteGuid = int.Parse(initObj.SiteGuid),
                    m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new SocialInviteInfo() { m_hashCode = hashCode } : null,
                    m_mediaID = mediaID,
                    m_mediaLink = mediaLink,
                    m_senderEmail = senderEmail,
                    m_senderName = senderName,
                    m_status = status,
                    m_voucherReceipents = voucherReceipents
                };

                res = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaign(initObj.SiteGuid, campaignID, actionInfo);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return res;
        }

    }
}