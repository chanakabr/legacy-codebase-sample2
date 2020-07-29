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
        public SuspendSettings SuspendSettings { get; set; }
        public PaymentGatewayStatus Status { get; set; }

        public HouseholdPaymentGateway()
        {
            this.SuspendSettings = new SuspendSettings();
        }

        public HouseholdPaymentGateway(HouseholdPaymentGateway householdPaymentGateway)
        {
            this.ChargeId = householdPaymentGateway.ChargeId;
            this.HouseholdId = householdPaymentGateway.HouseholdId;
            this.Selected = householdPaymentGateway.Selected;
            this.PaymentGatewayId = householdPaymentGateway.PaymentGatewayId;
            this.Status = householdPaymentGateway.Status;
            this.SuspendSettings = householdPaymentGateway.SuspendSettings;
        }
    }

    public class SuspendSettings
    {
        public bool RevokeEntitlements { get; set; }
        public bool StopRenew { get; set; }
    }
}
