using System;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFile
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public int Type { get; set; }
        public string Url { get; set; }
        public double Duration { get; set; }
        public string ExternalId { get; set; }
        public string AltExternalId { get; set; }
        public string ExternalStoreId { get; set; }                
        public long StreamingSupplierId { get; set; }
        public string AltStreamingCode { get; set; }
        public long AltStreamingSupplierId { get; set; }
        public string AdditionalData { get; set; }
        public long BillingType { get; set; }
        public int OrderNum { get; set; }
        public string Language { get; set; }
        public bool IsDefaultLanguage { get; set; }
        public int OutputProtecationLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long FileSize { get; set; }
        public bool? IsActive { get; set; }
    }
}