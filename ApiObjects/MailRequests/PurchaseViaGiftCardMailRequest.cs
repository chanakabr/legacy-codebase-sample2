using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class PurchaseViaGiftCardMailRequest : MailRequestObj
    {
        public string offerType;
        public string m_sItemName;
        public string m_sPurchaseDate;
        public string m_sUserEmail;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> result = new List<MCGlobalMergeVars>();
            MCGlobalMergeVars nameMergeVar = new MCGlobalMergeVars();
            nameMergeVar.name = "FIRSTNAME";
            nameMergeVar.content = this.m_sFirstName;
            result.Add(nameMergeVar);
            MCGlobalMergeVars lastNameMergeVar = new MCGlobalMergeVars();
            lastNameMergeVar.name = "LASTNAME";
            lastNameMergeVar.content = this.m_sLastName;
            result.Add(lastNameMergeVar);
            MCGlobalMergeVars itemMergeVar = new MCGlobalMergeVars();
            itemMergeVar.name = "ITEMNAME";
            itemMergeVar.content = this.m_sItemName;
            result.Add(itemMergeVar);
            MCGlobalMergeVars dateMergeVar = new MCGlobalMergeVars();
            dateMergeVar.name = "DATEOFPURCHASE";
            dateMergeVar.content = this.m_sPurchaseDate;
            result.Add(dateMergeVar);
            MCGlobalMergeVars userEmailMergeVar = new MCGlobalMergeVars();
            userEmailMergeVar.name = "USEREMAIL";
            userEmailMergeVar.content = this.m_sUserEmail;
            result.Add(userEmailMergeVar);
            MCGlobalMergeVars mergeType = new MCGlobalMergeVars()
            {
                name = "offerType",
                content = this.offerType
            };

            result.Add(mergeType);

            return result;
        }
    }
}
