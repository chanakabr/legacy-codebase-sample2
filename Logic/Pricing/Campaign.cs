using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class Campaign
    {

        private int m_groupID;
        public string m_Name;

        public long m_ID;

        public string m_Description;

        public UsageModule m_usageModule;

        public DateTime m_startDate;

        public DateTime m_endDate;

        public CouponsGroup m_oCouponsGroup;

        public CampaignTrigger m_CampaignTrigger;

        public CampaignResult m_CampaignResult;

        public CampaignType m_CampaignType;

        public int m_subscriptionCode;

        public bool m_IsActive;

        public Campaign()
        {
        }

        public CouponsGroup GetCampaignCouponGroup()
        {
            return m_oCouponsGroup;
        }

        public Campaign(long id, int groupID)
        {
            m_ID = id;
            m_groupID = groupID;
            Initialize();
        }

        private void Initialize()
        {
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                if (m_ID > 0)
                {
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("main_connection_string");
                    selectQuery += " select * from campaigns where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", m_ID);
                    selectQuery += " and status = 1";
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            string m_Name = selectQuery.Table("query").DefaultView[0].Row["Name"].ToString();
                            m_startDate = (DateTime)selectQuery.Table("query").DefaultView[0].Row["start_date"];
                            int nIsActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
                            if (nIsActive > 0)
                            {
                                m_IsActive = true;
                            }
                            else
                            {
                                m_IsActive = false;
                            }
                            object oEndDate = selectQuery.Table("query").DefaultView[0].Row["end_date"];
                            if (oEndDate != null && oEndDate != System.DBNull.Value)
                            {
                                m_endDate = (DateTime)oEndDate;
                            }
                            else
                            {
                                m_endDate = DateTime.MaxValue;
                            }
                            object oUM = selectQuery.Table("query").DefaultView[0].Row["usage_module_id"];
                            if (oUM != null && oUM != System.DBNull.Value)
                            {
                                string sUsageModuleCode = oUM.ToString();
                                if (sUsageModuleCode != "")
                                {
                                    BaseUsageModule um = null;
                                    Utils.GetBaseImpl(ref um, m_groupID);
                                    if (um != null)
                                        m_usageModule = um.GetUsageModuleData(sUsageModuleCode);
                                    else
                                        m_usageModule = null;
                                }
                                else
                                    m_usageModule = null;
                            }

                            object oCG = selectQuery.Table("query").DefaultView[0].Row["coupon_group_id"];
                            if (oCG != null && oCG != System.DBNull.Value)
                            {
                                string sCouponGroupCode = oCG.ToString();
                                if (sCouponGroupCode != "")
                                {
                                    BaseCoupons c = null;
                                    Utils.GetBaseImpl(ref c, m_groupID);
                                    if (c != null)
                                        m_oCouponsGroup = c.GetCouponGroupData(sCouponGroupCode);
                                    else
                                        m_oCouponsGroup = null;
                                }
                                else
                                    m_oCouponsGroup = null;
                            }

                            object oSC = selectQuery.Table("query").DefaultView[0].Row["default_sub"];
                            if (oSC != null && oSC != System.DBNull.Value)
                            {
                                m_subscriptionCode = int.Parse(oSC.ToString());
                            }

                            m_CampaignType = (CampaignType)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["campaign_type"].ToString()));
                            m_CampaignResult = (CampaignResult)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["result_type"].ToString()));
                            m_CampaignTrigger = (CampaignTrigger)(int.Parse(selectQuery.Table("query").DefaultView[0].Row["trigger_type"].ToString()));

                        }
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

        }
    }
}
