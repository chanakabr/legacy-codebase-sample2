namespace ApiObjects
{
    public class IngestProfileAdapterParam
    {
        [DBFieldMapping("ingest_profile_id")]
        public int IngestProfileId { get; set; }

        [DBFieldMapping("key")]
        public string Key { get; set; }

        [DBFieldMapping("value")]
        public string Value { get; set; }
    }
}