using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DomainBillingTransactionsResponse
    {
        public int domainID { get; set; }

        public UserBillingTransactionsResponse[] billingTransactionResponses { get; set; }
    }
}
