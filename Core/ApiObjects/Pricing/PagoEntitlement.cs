using System;

namespace ApiObjects
{
    [Serializable]
    public class PagoEntitlement
    {
        public long Id { get; set; }
        public long PurchasedByUserId { get; set; }
        public long PagoId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsPending { get; set; }
    }
}