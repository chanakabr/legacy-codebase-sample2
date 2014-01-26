using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class BillingTransactionsResponse
    {
        public BillingTransactionContainer[] transactions { get; set; }
       
        public int transactions_count { get; set; }
    }
}
