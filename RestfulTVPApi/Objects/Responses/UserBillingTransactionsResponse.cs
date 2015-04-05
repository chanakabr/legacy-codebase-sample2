using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class UserBillingTransactionsResponse
    {
        public string site_guid { get; set; }
        
        public BillingTransactionsResponse billing_transaction_response { get; set; }
    }
}
