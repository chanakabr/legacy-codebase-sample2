using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserBillingTransactionsResponse
    {
        public string m_sSiteGUID { get; set; }
        
        public BillingTransactionsResponse m_BillingTransactionResponse { get; set; }
    }
}
