using System.Collections.Generic;
using System.Runtime.Serialization;
using AdapaterCommon.Models;

namespace PlaybackAdapter
{
    [DataContract]
    public class MediaFile : AssetFile
    {
        [DataMember]
        public int? AssetId { get; set; }
        [DataMember]
        public int? Id { get; set; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public int? TypeId { get; set; }
        [DataMember]
        public long? Duration { get; set; }
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public string AltExternalId { get; set; }
        [DataMember]
        public long? FileSize { get; set; }
        [DataMember]
        public string AdditionalData { get; set; }
        [DataMember]
        public string AltStreamingCode { get; set; }
        [DataMember]
        public long? AlternativeCdnAdapaterProfileId { get; set; }
        [DataMember]
        public long? EndDate { get; set; }
        [DataMember]
        public long? StartDate { get; set; }
        [DataMember]
        public string ExternalStoreId { get; set; }
        [DataMember]
        public bool? IsDefaultLanguage { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public int? OrderNum { get; set; }
        [DataMember]
        public string OutputProtecationLevel { get; set; }
        [DataMember]
        public long? CdnAdapaterProfileId { get; set; }
        [DataMember]
        public bool? Status { get; set; }
        [DataMember]
        public long? CatalogEndDate { get; set; }
        [DataMember]
        public string Labels { get; set; }
        [DataMember]
        public List<KeyListOfStrings> DynamicData { get; set; }
    }
}
