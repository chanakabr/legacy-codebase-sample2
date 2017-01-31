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

            return result;
        }
    }
}
