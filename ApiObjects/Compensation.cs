using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects
{
    public class Compensation
    {
        public CompensationType CompensationType { get; set; }
       
        public double Amount { get; set; }

        public int TotalRenewals { get; set; }

        public int Renewals { get; set; }

        public long Id { get; set; }

        public long SubscriptionId { get; set; }

        public int PurchaseId { get; set; }
    }

    public class CompensationResponse
    {
        public Compensation Compensation { get; set; }

        public Status Status { get; set; }
    }
}
