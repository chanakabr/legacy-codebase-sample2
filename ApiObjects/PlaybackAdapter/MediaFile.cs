namespace ApiObjects.PlaybackAdapter
{
    public class MediaFile : AssetFile
    {

        public int? AssetId { get; set; }

        public int? Id { get; set; }

        public string Type { get; set; }

        public int? TypeId { get; set; }

        public string Url { get; set; }

        public long? Duration { get; set; }

        public string ExternalId { get; set; }

        public string AltExternalId { get; set; }

        public string BillingType { get; set; }

        public string Quality { get; set; }

        public string HandlingType { get; set; }

        public string CdnName { get; set; }

        public string CdnCode { get; set; }

        public string AltCdnCode { get; set; }

        public StringValueArray PPVModules { get; set; }

        public string ProductCode { get; set; }

        public long? FileSize { get; set; }

        public string AdditionalData { get; set; }

        public string AltStreamingCode { get; set; }

        public long? AlternativeCdnAdapaterProfileId { get; set; }

        public long? EndDate { get; set; }

        public long? StartDate { get; set; }

        public string ExternalStoreId { get; set; }

        public bool? IsDefaultLanguage { get; set; }

        public string Language { get; set; }

        public int? OrderNum { get; set; }

        public string OutputProtecationLevel { get; set; }

        public long? CdnAdapaterProfileId { get; set; }

        public bool? Status { get; set; }

        public long? CatalogEndDate { get; set; }
    }
}