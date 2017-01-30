using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class GiftCardReminderMailRequest : PurchaseMailRequest
    {
        public string daysLeft;
        public string endDate;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> result = base.getRequestMergeObj();

            MCGlobalMergeVars mergeDaysLeft = new MCGlobalMergeVars()
            {
                name = "daysLeft",
                content = this.daysLeft
            };

            result.Add(mergeDaysLeft);

            MCGlobalMergeVars mergeEndDate = new MCGlobalMergeVars()
            {
                name = "endDate",
                content = this.endDate
            };

            result.Add(mergeEndDate);

            MCGlobalMergeVars mergeOfferName = new MCGlobalMergeVars()
            {
                name = "subscriptionName",
                content = this.m_sItemName
            };

            result.Add(mergeOfferName);

            MCGlobalMergeVars mergeEmail = new MCGlobalMergeVars()
            {
                name = "email",
                content = this.m_sSenderTo
            };

            result.Add(mergeEmail);

            MCGlobalMergeVars mergeFirstName = new MCGlobalMergeVars()
            {
                name = "firstName",
                content = this.m_sFirstName
            };

            result.Add(mergeFirstName);

            MCGlobalMergeVars mergeLastName = new MCGlobalMergeVars()
            {
                name = "lastName",
                content = this.m_sLastName
            };

            result.Add(mergeLastName);

            return result;
        }
    }
}
