using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
    public enum KalturaReportOrderBy
    {
        NONE
    }
    public class KalturaReportFilter : KalturaFilter<KalturaReportOrderBy>
    {
        [DataMember(Name = "fromDateEqual")]
        [JsonProperty("fromDateEqual")]
        [XmlElement(ElementName = "fromDateEqual")]
        public long FromDateEqual { get; set; }

        public override KalturaReportOrderBy GetDefaultOrderByValue()
        {
            return KalturaReportOrderBy.NONE;
        }    
    }
}