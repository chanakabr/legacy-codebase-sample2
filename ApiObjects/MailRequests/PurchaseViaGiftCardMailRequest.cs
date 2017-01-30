using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class PurchaseViaGiftCardMailRequest : PurchaseMailRequest
    {
        public string offerType;

        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> result = base.getRequestMergeObj();

            MCGlobalMergeVars mergeType = new MCGlobalMergeVars()
            {
                name = "offerType",
                content = this.offerType
            };

            result.Add(mergeType);

            MCGlobalMergeVars mergeOfferName = new MCGlobalMergeVars()
            {
                name = "offerName",
                content = this.m_sItemName
            };

            result.Add(mergeOfferName);

            MCGlobalMergeVars mergePrice = new MCGlobalMergeVars()
            {
                name = "price",
                content = this.m_sPrice
            };

            result.Add(mergePrice);

            MCGlobalMergeVars mergePurchaseDate = new MCGlobalMergeVars()
            {
                name = "purchaseDate",
                content = this.m_sPurchaseDate
            };

            result.Add(mergePurchaseDate);

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
