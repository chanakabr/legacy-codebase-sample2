using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    public class TvinciCampaign : BaseCampaign
    {

       // protected BaseCampaignActionImpl m_campaignActionImpl;

        public TvinciCampaign(int groupID)
            : base(groupID)
        {
            
        }

        public override Campaign[] GetCampaignsByType(CampaignTrigger triggerType)
        {
            Campaign[] retVal = null;
            List<Campaign> tempRet = new List<Campaign>();
            int typeInt = (int)triggerType;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                if (m_nGroupID > 0)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("main_connection_string");
                    selectQuery += "select id from campaigns with (nolock) where is_active = 1 and status = 1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("trigger_type", "=", typeInt);
                    selectQuery += " and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                int campID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());
                                Campaign camp = new Campaign(campID, m_nGroupID);
                                tempRet.Add(camp);
                            }
                        }
                    }


                }
                if (tempRet.Count > 0)
                {
                    retVal = tempRet.ToArray();
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return retVal;
        }

        public override Campaign GetCampaignByHash(string hash)
        {
            Campaign retVal = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select voucher_campaign_id with (nolock) from coupons where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("code", "=", hash);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        long campID = long.Parse(selectQuery.Table("query").DefaultView[0].Row["voucher_campaign_id"].ToString());
                        retVal = GetCampaignData(campID);
                    }
                }

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return retVal;
        }

        public override Campaign[] GetMediaCampaigns(int mediaID)
        {
            Campaign[] retVal = null;
            List<Campaign> tempRet = new List<Campaign>();
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                if (m_nGroupID > 0)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("main_connection_string");

                    selectQuery += "select cc.channel_id, cc.campaign_id from campaigns_channels cc, campaigns c, channels ch where cc.is_active = 1 and cc.status = 1 and";
                    selectQuery += "cc.campaign_id = c.id and c.is_active = 1 and c.status = 1 and ch.id = cc.channel_id and ch.is_Active = 1 and ch.status = 1";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            Dictionary<int, List<int>> campaignChannels = new Dictionary<int, List<int>>();
                            for (int i = 0; i < count; i++)
                            {
                                int campaignID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["campaign_id"].ToString());
                                int channelID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["channel_id"].ToString());
                                if (!campaignChannels.ContainsKey(campaignID))
                                {
                                    campaignChannels.Add(campaignID, new List<int>());
                                }
                                campaignChannels[campaignID].Add(channelID);
                            }
                            foreach (KeyValuePair<int, List<int>> kv in campaignChannels)
                            {
                                if (kv.Value.Count > 0)
                                {
                                    if (Api.Module.DoesMediaBelongToChannels(m_nGroupID, kv.Value.ToArray(), null, mediaID, true, ""))
                                    {
                                        Campaign camp = new Campaign(kv.Key, m_nGroupID);
                                        tempRet.Add(camp);

                                    }
                                }
                            }
                        }
                    }
                }
                if (tempRet.Count > 0)
                {
                    retVal = tempRet.ToArray();
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return retVal;
        }
    }
}
