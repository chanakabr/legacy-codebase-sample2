namespace ApiObjects.SSOAdapter
{
    public class SSOAdapterParam
    {
        [DBFieldMapping("sso_adapter_id")]
        public int AdapterId { get; set; }

        [DBFieldMapping("group_id")]
        public int GroupId { get; set; }

        [DBFieldMapping("keyName")]
        public string Key { get; set; }

        [DBFieldMapping("value")]
        public string Value { get; set; }
    }
}