using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class GiftCardReminderMailRequest : MailRequestObj
    {
        public string daysLeft;
        public string endDate;
        public string itemName;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> result = new List<MCGlobalMergeVars>();

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
                content = this.itemName
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
