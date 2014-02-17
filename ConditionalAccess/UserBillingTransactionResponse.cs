using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public class UserBillingTransactionsResponse
    {
        public string m_sSiteGUID;
        public BillingTransactionsResponse m_BillingTransactionResponse;

        public UserBillingTransactionsResponse() { }
    }

    public class DomainBillingTransactionsResponse
    {
        public int m_nDomainID;

        public UserBillingTransactionsResponse[] m_BillingTransactionResponses;

        public DomainBillingTransactionsResponse() { }
    }

}
