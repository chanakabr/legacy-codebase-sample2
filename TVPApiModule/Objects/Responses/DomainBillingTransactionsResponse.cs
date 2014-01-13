using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DomainBillingTransactionsResponse
    {
        public int m_nDomainID { get; set; }

        public UserBillingTransactionsResponse[] m_BillingTransactionResponses { get; set; }
    }
}
