using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class UpdatePendingResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public string BillingGuid { get; set; }

        public eTransactionType ProductType { get; set; }

        public eTransactionState TransactionState { get; set; }

        public long DomainId { get; set; }
    }
}
