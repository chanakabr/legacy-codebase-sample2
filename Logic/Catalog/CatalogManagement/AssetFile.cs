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
        public string ExternalStoreId { get; set; }
        public string Quality { get; set; }
        public string StreamingCode { get; set; }
        public string StreamingSuplierId { get; set; }
        public string AltStreamingCode { get; set; }
        public string AltStreamingSuplierId { get; set; }
        public string AdditionalData { get; set; }
        public string BillingType { get; set; }
        public int OrderNum { get; set; }
        public string Language { get; set; }
        public string IsDefaultLanguage { get; set; }
        public string OutputProtecationLevel { get; set; }
        public long StartDate { get; set; }
        public long EndDate { get; set; }
        public long FileSize { get; set; }
    }
}