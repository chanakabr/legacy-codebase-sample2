using ApiObjects;
using ApiObjects.Base;
using System.Collections.Generic;

namespace DAL
{
    public interface ICampaignRepository
    {
        bool UpdateCampaign(Campaign campaign, ContextData contextData);
        List<CampaignDB> GetCampaignsByGroupId(int groupId, eCampaignType campaignType);
        Campaign GetCampaignById(int groupId, long id);
        T AddCampaign<T>(T campaign, ContextData contextData) where T : Campaign, new();
        bool DeleteCampaign(long groupId, long campaignId);
    }
}
