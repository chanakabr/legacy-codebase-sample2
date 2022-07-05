using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Exceptions;

namespace WebAPI.Models.API
{
    public partial class KalturaRegionChannelNumberMultiLcns : KalturaRegionChannelNumber
    {
        /// <summary>
        /// Linear channel numbers
        /// </summary>
        [DataMember(Name = "lcns")]
        [JsonProperty("lcns")]
        [XmlElement(ElementName = "lcns")]
        public string LCNs { get; set; }
    }
}