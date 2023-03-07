using System.Collections.Generic;

namespace ApiObjects.PlaybackAdapter
{
    public class MediaFile : AssetFile
    {
        public int? AssetId { get; set; }

        public int? Id { get; set; }

        public string Type { get; set; }

        public int? TypeId { get; set; }

        public long? Duration { get; set; }

        public string ExternalId { get; set; }

        public string AltExternalId { get; set; }

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

        public string Labels { get; set; }

        public IDictionary<string, IEnumerable<string>> DynamicData { get; set; }
    }
}