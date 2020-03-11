using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CinepolisRenewalFailMailRequest : MailRequestObj
    {
        public string m_sPurchaseDate;
        public string m_sItemName;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            switch (this.m_eMailType)
            {
                case eMailTemplateType.PaymentFail:
                    {
                        MCGlobalMergeVars usernameMergeVar = new MCGlobalMergeVars();
                        usernameMergeVar.name = "FIRSTNAME";
                        usernameMergeVar.content = this.m_sFirstName;
                        retVal.Add(usernameMergeVar);
                        MCGlobalMergeVars itemMergeVar = new MCGlobalMergeVars();
                        itemMergeVar.name = "ITEMNAME";
                        itemMergeVar.content = this.m_sItemName;
                        retVal.Add(itemMergeVar);
                        MCGlobalMergeVars dateMergeVar = new MCGlobalMergeVars();
                        dateMergeVar.name = "DATEOFPURCHASE";
                        dateMergeVar.content = this.m_sPurchaseDate;
                        retVal.Add(dateMergeVar);
                        break;
                    }
                default:
                    break;
            }

            return retVal;
        }
    }
}
