using System;

namespace Core.Catalog.CatalogManagement
{
    public class AssetFile
    {
        public long Id { get; set; }
        public long AssetId { get; set; }
        public int? TypeId { get; set; }
        public string Url { get; set; }
        public long? Duration { get; set; }
        public string ExternalId { get; set; }
        public string AltExternalId { get; set; }
        public string ExternalStoreId { get; set; }                
        public long? CdnAdapaterProfileId { get; set; }
        public string AltStreamingCode { get; set; }
        public long? AlternativeCdnAdapaterProfileId { get; set; }
        public string AdditionalData { get; set; }
        public long BillingType { get; set; }
        public int? OrderNum { get; set; }
        public string Language { get; set; }
        public bool? IsDefaultLanguage { get; set; }
        public int OutputProtecationLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? FileSize { get; set; }
        public bool? IsActive { get; set; }

        private string type;

        public AssetFile()
        {
            type = string.Empty;
        }

        public AssetFile(string typeName)
        {
            type = typeName;
        }

        public string GetTypeName()
        {
            return type;
        }
    }
}