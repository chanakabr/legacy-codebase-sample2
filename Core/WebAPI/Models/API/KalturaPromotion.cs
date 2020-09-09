using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Promotion
    /// </summary>
    public partial class KalturaPromotion : KalturaOTTObject
    {
        /// <summary>
        /// The discount module id that is promoted to the user
        /// </summary>
        [DataMember(Name = "discountModuleId")]
        [JsonProperty("discountModuleId")]
        [XmlElement(ElementName = "discountModuleId")]
        [SchemeProperty(MinLong = 1)]
        public long DiscountModuleId { get; set; }

        /// <summary>
        /// These conditions define the Promotion that applies on
        /// </summary>
        [DataMember(Name = "conditions")]
        [JsonProperty("conditions")]
        [XmlElement(ElementName = "conditions")]
        public List<KalturaCondition> Conditions { get; set; }

        /// <summary>
        /// the numer of recurring for this promotion
        /// </summary>
        [DataMember(Name = "numberOfRecurring")]
        [JsonProperty("numberOfRecurring")]
        [XmlElement(ElementName = "numberOfRecurring")]
        public int? NumberOfRecurring { get; set; }

        internal void Validate()
        {
            if (this.Conditions == null || this.Conditions.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "discountConditions");
            }

            foreach (var condition in this.Conditions)
            {
                condition.Validate();
            }
        }
    }
}