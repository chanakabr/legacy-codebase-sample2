using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.ConditionalAccess.Response
{
    public class BillingTransactions
    {
        public ApiObjects.Response.Status resp { get; set; }
        public BillingTransactionsResponse transactions { get; set; }

        public BillingTransactions()
        {
            resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            transactions = new BillingTransactionsResponse();
        }

        public BillingTransactions(ApiObjects.Response.Status resp, BillingTransactionsResponse transactions)
        {
            this.resp = resp;
            this.transactions = transactions;
        }
    }
}
