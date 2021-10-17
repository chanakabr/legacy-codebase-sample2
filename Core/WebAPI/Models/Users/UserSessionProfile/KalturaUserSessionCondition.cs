using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;

namespace WebAPI.Models.Users.UserSessionProfile
{
    /// <summary>
    /// SimpleExpression hold single condition
    /// </summary>
    public partial class KalturaUserSessionCondition : KalturaUserSessionProfileExpression
    {
        private static readonly HashSet<KalturaRuleConditionType> VALID_CONDITIONS = new HashSet<KalturaRuleConditionType>()
        {
            KalturaRuleConditionType.SEGMENTS,
            KalturaRuleConditionType.DYNAMIC_KEYS,
            KalturaRuleConditionType.DEVICE_BRAND,
            KalturaRuleConditionType.DEVICE_FAMILY,
            KalturaRuleConditionType.DEVICE_MANUFACTURER,
            KalturaRuleConditionType.DEVICE_MODEL,
            KalturaRuleConditionType.DEVICE_DYNAMIC_DATA
        };

        /// <summary>
        /// expression
        /// </summary>
        [DataMember(Name = "condition")]
        [JsonProperty("condition")]
        [XmlElement(ElementName = "condition")]
        public KalturaCondition Condition { get; set; }

        internal override int ConditionsSum()
        {
            return Condition.ConditionsCount();
        }

        internal override void Validate()
        {
            if (Condition == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "condition");
            }

            if (!VALID_CONDITIONS.Contains(Condition.Type))
            {
                throw new BadRequestException(BadRequestException.TYPE_NOT_SUPPORTED, "condition", Condition.objectType);
            }

            Condition.Validate(VALID_CONDITIONS);
        }
    }
}