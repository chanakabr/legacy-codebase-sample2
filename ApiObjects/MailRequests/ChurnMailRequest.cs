using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class ChurnMailRequest : MailRequestObj
    {
        public string CouponCode;
        
        public override List<MCGlobalMergeVars> getRequestMergeObj()
        {
            List<MCGlobalMergeVars> result = new List<MCGlobalMergeVars>();

            MCGlobalMergeVars couponCode = new MCGlobalMergeVars()
            {
                name = "couponCode",
                content = this.CouponCode
            };

            result.Add(couponCode);

            return result;
        }
    }
}
