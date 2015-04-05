using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class DomainBillingTransactionsResponse
    {
        public int domain_id { get; set; }

        public UserBillingTransactionsResponse[] billing_transaction_responses { get; set; }
    }
}
