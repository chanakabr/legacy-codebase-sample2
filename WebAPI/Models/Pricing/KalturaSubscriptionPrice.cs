using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    /// <summary>
    /// Subscription price details
    /// </summary>
    [OldStandard("purchaseStatus", "purchase_status")]
    [OldStandard("price", "price")]
    public class KalturaSubscriptionPrice : KalturaProductPrice
    {
        /// <summary>
        /// Product price 
        /// </summary>
        [DataMember(Name = "price")]
        [JsonProperty("price")]
        [XmlElement(ElementName = "price", IsNullable = true)]
        [Obsolete]
        public KalturaPrice Price { get { return base.Price; } set { base.Price = value; }}

        /// <summary>
        /// Product purchase status  
        /// </summary>
        [DataMember(Name = "purchaseStatus")]
        [JsonProperty("purchaseStatus")]
        [XmlElement(ElementName = "purchaseStatus")]
        [Obsolete]
        public KalturaPurchaseStatus PurchaseStatus { get { return base.PurchaseStatus; } set { base.PurchaseStatus = value; } }
    }
}