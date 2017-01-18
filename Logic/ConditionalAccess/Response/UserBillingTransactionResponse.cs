using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess
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

    public class DomainsBillingTransactionsResponse
    {
        public ApiObjects.Response.Status status;

        public DomainBillingTransactionsResponse[] billingTransactions;

        public DomainsBillingTransactionsResponse()
        {

        }
    }

    public class DomainTransactionsHistoryResponse
    {
        public ApiObjects.Response.Status Status;

        public int TransactionsCount;

        public List<TransactionHistoryContainer> TransactionsHistory;

        public DomainTransactionsHistoryResponse()
        {
            TransactionsHistory = new List<TransactionHistoryContainer>();
        }
    }

}