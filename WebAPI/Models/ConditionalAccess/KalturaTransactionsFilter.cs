using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Transactions filter
    /// </summary>
    [Obsolete]
    public class KalturaTransactionsFilter : KalturaFilterPager
    {
        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "by")]
        [JsonProperty("by")]
        [XmlElement(ElementName = "by")]
        public KalturaEntityReferenceBy By
        {
            get;
            set;
        }

        /// <summary>
        ///Filter transactions later than specific date
        /// </summary>
        [DataMember(Name = "startDate")]
        [JsonProperty("startDate")]
        [XmlElement(ElementName = "startDate", IsNullable = true)]
        [OldStandardProperty("start_date")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///Filter transactions earlier than specific date
        /// </summary>
        [DataMember(Name = "endDate")]
        [JsonProperty("endDate")]
        [XmlElement(ElementName = "endDate", IsNullable = true)]
        [OldStandardProperty("end_date")]
        public DateTime? EndDate { get; set; }
    }
}