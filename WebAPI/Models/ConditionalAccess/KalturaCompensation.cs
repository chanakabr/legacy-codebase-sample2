using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Compensation request parameters
    /// </summary>
    public class KalturaCompensation : KalturaOTTObject
    {
        /// <summary>
        /// C-DVR adapter identifier
        /// </summary>
        [DataMember(Name = "compensationType")]
        [JsonProperty("compensationType")]
        [XmlElement(ElementName = "compensationType")]
        public KalturaCompensationType CompensationType { get; set; }

        /// <summary>
        /// C-DVR adapter identifier
        /// </summary>
        [DataMember(Name = "amount")]
        [JsonProperty("amount")]
        [XmlElement(ElementName = "amount")]
        public int Amount { get; set; }

        /// <summary>
        /// C-DVR adapter identifier
        /// </summary>
        [DataMember(Name = "renewalIterations")]
        [JsonProperty("renewalIterations")]
        [XmlElement(ElementName = "renewalIterations")]
        public int RenewalIterations { get; set; }
    }

    /// <summary>
    /// Compensation type
    /// </summary>
    public enum KalturaCompensationType
    {
        PERCENTAGE = 0,
        FIXED_AMOUNT = 1
    }
}