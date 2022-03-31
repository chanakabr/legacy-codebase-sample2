using ApiObjects.Billing;
using System;

namespace ApiObjects.ConditionalAccess.DTO
{
    public class EntitlementDto
    {
        public eTransactionType Type { get; set; }
        public long EntitlementId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CurrentDate { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int PurchaseId { get; set; }
        public int BillingTransactionId { get; set; }
        public string BillingGuid { get; set; }
        public string DeviceUdid { get; set; }
        public bool IsPending { get; set; }
        public long UserId { get; set; }
        public long HouseholdId { get; set; }
    }
}