using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserBillingTransactionsResponse
    {
        public string siteGUID { get; set; }
        
        public BillingTransactionsResponse billingTransactionResponse { get; set; }
    }
}
