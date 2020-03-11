using ApiObjects.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{

    
    public abstract class BaseCampaign
    {
        protected int m_nGroupID;

        public BaseCampaign(int groupID)
        {
            m_nGroupID = groupID;
        }

        public abstract Campaign[] GetMediaCampaigns(int mediaID);

        public abstract Campaign[] GetCampaignsByType(CampaignTrigger triggerType);

        public abstract Campaign GetCampaignByHash(string hash);

        public virtual Campaign GetCampaignData(long campaignID)
        {
            Campaign retVal = new Campaign();
            if (campaignID > 0)
            {
                retVal = new Campaign(campaignID, m_nGroupID);
            }
            return retVal;
        }

    }
}
