using Newtonsoft.Json;
using System;

namespace Core.Catalog.CatalogManagement
{
    [Serializable]
    public class AssetFile
    {
        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public long Id { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public long AssetId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public int? TypeId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public string Url { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public long? Duration { get; set; }

        [ExcelTemplateAttribute(PropertyValueRequired = true)]
        public string ExternalId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public string AltExternalId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public string ExternalStoreId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public long? CdnAdapaterProfileId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public string AltStreamingCode { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public long? AlternativeCdnAdapaterProfileId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public string AdditionalData { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public long BillingType { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public int? OrderNum { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public string Language { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public bool? IsDefaultLanguage { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public int OutputProtecationLevel { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public DateTime? StartDate { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public DateTime? EndDate { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public long? FileSize { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = false)]
        public bool? IsActive { get; set; }

        [JsonProperty("type")]
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