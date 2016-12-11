using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public enum KalturaReminderOrderBy
    {
        NONE
    }

    public class KalturaReminderFilter : KalturaFilter<KalturaReminderOrderBy>
    {

        public override KalturaReminderOrderBy GetDefaultOrderByValue()
        {
            return KalturaReminderOrderBy.NONE;
        }

        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }
    }
}