using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class CinepolisPurchaseMailRequest : MailRequestObj
    {
        public string m_sPurchaseDate;
        public string m_sItemName;
        public string m_sPrice;
        public string m_sUsername;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> retVal = new List<MCGlobalMergeVars>();
            switch (this.m_eMailType)
            {
                case eMailTemplateType.Purchase:
                    {
                        MCGlobalMergeVars usernameMergeVar = new MCGlobalMergeVars();
                        usernameMergeVar.name = "USERNAME";
                        usernameMergeVar.content = this.m_sUsername;
                        retVal.Add(usernameMergeVar);
                        MCGlobalMergeVars priceMergeVar = new MCGlobalMergeVars();
                        priceMergeVar.name = "CATPRICE";
                        priceMergeVar.content = this.m_sPrice;
                        retVal.Add(priceMergeVar);
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
