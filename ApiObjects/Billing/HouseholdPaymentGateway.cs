using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Billing
{
    public class HouseholdPaymentGateway
    {
        public int PaymentGatewayId { get; set; }
        public long HouseholdId { get; set; }
        public int Selected { get; set; }
        public string ChargeId { get; set; }

        public HouseholdPaymentGateway()
        {

        }
        public HouseholdPaymentGateway(HouseholdPaymentGateway householdPaymentGateway)
        {
            this.ChargeId = householdPaymentGateway.ChargeId;
            this.HouseholdId = householdPaymentGateway.HouseholdId;
            this.Selected = householdPaymentGateway.Selected;
            this.PaymentGatewayId = householdPaymentGateway.PaymentGatewayId;
        }
    }
}
