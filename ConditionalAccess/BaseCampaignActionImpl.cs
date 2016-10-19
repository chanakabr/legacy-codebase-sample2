using Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public abstract class BaseCampaignActionImpl
    {

        public virtual bool ActivateCampaign(Campaign camp, CampaignActionInfo cai, int groupID)
        {
            bool retVal = false;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("campaigns_uses");
                updateQuery += "num_of_uses = num_of_uses + 1";
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", camp.m_ID);
                updateQuery += "and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", cai.m_siteGuid);
                updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }
            return retVal;
        }

        public virtual CampaignActionInfo ActivateCampaignWithInfo(Campaign camp, CampaignActionInfo cai, int groupID)
        {
            return null;
        }

        protected virtual bool IsCampaignValid(long siteGuid, long campaignID, ref int numOfUsers)
        {
            bool retVal = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select id, num_of_uses from campaigns_uses where num_of_uses < max_num_of_uses and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("site_guid", "=", siteGuid);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("campaign_id", "=", campaignID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = true;
                        numOfUsers = int.Parse(selectQuery.Table("query").DefaultView[0].Row["num_of_uses"].ToString());
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

        
    }
}
