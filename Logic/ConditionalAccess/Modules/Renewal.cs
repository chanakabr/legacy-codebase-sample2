using ApiObjects;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.ConditionalAccess.Modules
{
    public class UnifiedPaymentRenewal : CoreObject
    {
        public DateTime Date { get; set; }

        public Price Price { get; set; }

        public long UnifiedPaymentId { get; set; }

        public List<EntitlementRenewalBase> Entitlements { get; set; }

        public UnifiedPaymentRenewal()
        {

        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }
    }

    public class EntitlementRenewal : CoreObject
    {
        public DateTime Date { get; set; }

        public Price Price { get; set; }

        public long PurchaseId { get; set; }

        public long SubscriptionId { get; set; }

        public EntitlementRenewal()
        {

        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }

    public class EntitlementRenewalBase : CoreObject
    {
        public long PurchaseId { get; set; }

        public long SubscriptionId { get; set; }

        public double PriceAmount { get; set; }

        public EntitlementRenewalBase()
        {

        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }
    }
}
