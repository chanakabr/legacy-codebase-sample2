using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.DMS
{
   
    /// <summary>
    /// Report filter
    /// </summary>
    public class KalturaDeviceReportFilter : KalturaReportFilter
    {
        [DataMember(Name = "lastAccessDateGreaterThanOrEqual")]
        [JsonProperty("lastAccessDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "lastAccessDateGreaterThanOrEqual")]
        public long LastAccessDateGreaterThanOrEqual { get; set; }

      
    }
}