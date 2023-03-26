using ApiObjects;
using ApiObjects.Base;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DAL
{
    public class CampaignRepository : ICampaignRepository
    {
        private static readonly Lazy<CampaignRepository> lazy = new Lazy<CampaignRepository>(() => new CampaignRepository(), LazyThreadSafetyMode.PublicationOnly);

        public static ICampaignRepository Instance { get { return lazy.Value; } }

        private CampaignRepository()
        {
        }

        public T AddCampaign<T>(T campaign, ContextData contextData) where T : Campaign, new()
        {
            campaign.UpdaterId = contextData.UserId.Value;

            var sp = new StoredProcedure("Insert_Campaign");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", contextData.GroupId);
            sp.AddParameter("@startDate", campaign.StartDate);
            sp.AddParameter("@endDate", campaign.EndDate);
            sp.AddParameter("@has_promotion", campaign.Promotion != null ? 1 : 0);
            sp.AddParameter("@type", (int)campaign.CampaignType);
            sp.AddParameter("@campaign_json", JsonConvert.SerializeObject(campaign));

            if (campaign.AssetUserRuleId.HasValue)
            {
                sp.AddParameter("@assetUserRuleId", campaign.AssetUserRuleId.Value);
            }

            var ds = sp.ExecuteDataSet();
            if (ds?.Tables?.Count > 0 && ds.Tables[0].Rows?.Count > 0)
            {
                var dr = ds.Tables[0].Rows[0];
                var response = JsonConvert.DeserializeObject<T>(Utils.GetSafeStr(dr, "campaign_json"));
                if (response != null)
                {
                    response.Id = Utils.GetLongSafeVal(dr, "ID");
                }

                return response;
            }

            return null;
        }

        public bool UpdateCampaign(Campaign campaign, ContextData contextData)
        {
            campaign.UpdaterId = contextData.UserId ?? 999;

            var sp = new StoredProcedure("Update_Campaign");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@id", campaign.Id);
            sp.AddParameter("@groupId", contextData.GroupId);
            sp.AddParameter("@startDate", campaign.StartDate);
            sp.AddParameter("@endDate", campaign.EndDate);
            sp.AddParameter("@has_promotion", campaign.Promotion != null ? 1 : 0);
            sp.AddParameter("@state", (int)campaign.State);
            sp.AddParameter("@campaign_json", JsonConvert.SerializeObject(campaign));

            if (campaign.AssetUserRuleId.HasValue)
            {
                sp.AddParameter("@assetUserRuleId", campaign.AssetUserRuleId.Value);
            }

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public bool DeleteCampaign(long groupId, long campaignId)
        {
            var sp = new StoredProcedure("Delete_Campaign");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@id", campaignId);
            sp.AddParameter("@groupId", groupId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        public List<CampaignDB> GetCampaignsByGroupId(int groupId, eCampaignType campaignType)
        {
            var sp = new StoredProcedure("Get_CampaignsByGroupId");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@campaignType", (int)campaignType);
            return sp.ExecuteDataSet().Tables[0].ToList<CampaignDB>();
        }

        public Campaign GetCampaignById(int groupId, long id)
        {
            Campaign response = null;
            var sp = new StoredProcedure("Get_CampaignById");
            sp.SetConnectionKey("pricing_connection");
            sp.AddParameter("@groupId", groupId);
            sp.AddParameter("@ID", id);

            var tb = sp.ExecuteDataSet().Tables[0];
            if (tb?.Rows != null && tb.Rows.Count > 0 && tb.Rows[0] != null)
            {
                var dr = tb.Rows[0];
                var type = Utils.GetIntSafeVal(dr, "type");
                if (type == (int)eCampaignType.Trigger)
                {
                    var triggerCampaign = JsonConvert.DeserializeObject<TriggerCampaign>(Utils.GetSafeStr(dr, "campaign_json"));
                    triggerCampaign.Id = Utils.GetLongSafeVal(dr, "id");
                    response = triggerCampaign;
                }
                else if (type == (int)eCampaignType.Batch)
                {
                    var batchCampaign = JsonConvert.DeserializeObject<BatchCampaign>(Utils.GetSafeStr(dr, "campaign_json"));
                    batchCampaign.Id = Utils.GetLongSafeVal(dr, "id");
                    response = batchCampaign;
                }
            }

            return response;
        }
    }
}
