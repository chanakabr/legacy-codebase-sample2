using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiObjects
{
    public class SmsAdapterProfile : ICrudHandeledObject
    {
        public long? Id { get; set; }
        
        [DBFieldMapping("group_id")]
        public int GroupId { get; set; }
        
        [DBFieldMapping("is_active")]
        public bool IsActive { get; set; }
        
        [DBFieldMapping("adapter_url")]
        public string AdapterUrl { get; set; }
        
        [DBFieldMapping("shared_secret")]
        public string SharedSecret { get; set; }
        public string Name { get; set; }

        [DBFieldMapping("external_identifier")]
        public string ExternalIdentifier { get; set; }
        
        public IList<SmsAdapterParam> Settings { get; set; }
    }

    public class SmsAdapterParam : ICrudHandeledObject
    {
        [DBFieldMapping("sms_adapter_id")]
        public int AdapterId { get; set; }

        [DBFieldMapping("group_id")]
        public int GroupId { get; set; }

        [DBFieldMapping("keyName")]
        public string Key { get; set; }

        [DBFieldMapping("value")]
        public string Value { get; set; }
    }
}
