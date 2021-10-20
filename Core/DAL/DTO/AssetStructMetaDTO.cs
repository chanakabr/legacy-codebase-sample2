namespace DAL.DTO
{
    public class AssetStructMetaDTO
    {
        public bool? IsInherited { get; set; }
        public bool? IsLocationTag { get; set; }
        public int? SuppressedOrder { get; set; }
        public string Alias { get; set; }
        public string IngestReferencePath { get; set; }
        public bool? ProtectFromIngest { get; set; }
        public string DefaultIngestValue { get; set; }
        public long AssetStructId { get; set; }
        public long MetaId { get; set; }
        public long CreateDate { get; set; }
        public long UpdateDate { get; set; }
    }
}
