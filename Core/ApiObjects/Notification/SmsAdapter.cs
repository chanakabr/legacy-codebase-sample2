using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Notification
{
    public class SmsAdapter
    {
        [DBFieldMapping("ID")]
        public int? Id { get; set; }

        [DBFieldMapping("group_id")]
        public int GroupId { get; set; }

        [DBFieldMapping("name")]
        public string Name { get; set; }

        [DBFieldMapping("is_active")]
        public int? IsActive { get; set; }

        [DBFieldMapping("adapter_url")]
        public string AdapterUrl { get; set; }

        public IList<SmsAdapterParam> Settings { get; set; }

        [DBFieldMapping("external_identifier")]
        public string ExternalIdentifier { get; set; }

        [DBFieldMapping("shared_secret")]
        public string SharedSecret { get; set; }
    }
}
