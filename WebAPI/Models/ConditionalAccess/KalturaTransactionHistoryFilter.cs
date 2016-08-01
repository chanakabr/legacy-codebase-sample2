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
    public enum KalturaTransactionHistoryOrderBy
    {
        CREATE_DATE_ASC,
        CREATE_DATE_DESC
    }

    /// <summary>
    /// Transactions filter
    /// </summary>
    public class KalturaTransactionHistoryFilter : KalturaFilter<KalturaTransactionHistoryOrderBy>
    {
        public override KalturaTransactionHistoryOrderBy GetDefaultOrderByValue()
        {
            return KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC;
        }

        /// <summary>
        ///Reference type to filter by
        /// </summary>
        [DataMember(Name = "entityReferenceEqual")]
        [JsonProperty("entityReferenceEqual")]
        [XmlElement(ElementName = "entityReferenceEqual", IsNullable = true)]
        public KalturaEntityReferenceBy EntityReferenceEqual
        {
            get;
            set;
        }

        /// <summary>
        ///Filter transactions later than specific date
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty("startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual", IsNullable = true)]
        public DateTime? StartDateGreaterThanOrEqual { get; set; }

        
        /// <summary>
        ///Filter transactions earlier than specific date
        /// </summary>
        [DataMember(Name = "endDateLessThanOrEqual")]
        [JsonProperty("endDateLessThanOrEqual")]
        [XmlElement(ElementName = "endDateLessThanOrEqual", IsNullable = true)]
        public DateTime? EndDateLessThanOrEqual { get; set; }
    }
}