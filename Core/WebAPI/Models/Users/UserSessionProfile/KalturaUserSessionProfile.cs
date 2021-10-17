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
    /// Define KalturaUserSessionProfileExpression
    /// </summary>
    public abstract partial class KalturaUserSessionProfileExpression : KalturaOTTObject
    {
        internal abstract void Validate();

        // Computes the total count of actual contions
        internal abstract int ConditionsSum();
    }

    /// <summary>
    /// User Session Profile
    /// </summary>
    public partial class KalturaUserSessionProfile : KalturaOTTObject
    {
        private const int MAX_CONDITIONS = 10;

        /// <summary>
        ///  The user session profile id.
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long Id { get; set; }

        /// <summary>
        /// The user session profile name for presentation.
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        [SchemeProperty(Pattern = SchemeInputAttribute.ASCII_ONLY_PATTERN)]
        public string Name { get; set; }

        /// <summary>
        /// expression
        /// </summary>
        [DataMember(Name = "expression")]
        [JsonProperty("expression")]
        [XmlElement(ElementName = "expression")]
        public KalturaUserSessionProfileExpression Expression { get; set; }

        internal void ValidateForAdd()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            if (Expression == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "expression");
            }

            ValidateExpression();
        }

        internal void ValidateForUpdate()
        {
            if (Expression != null)
            {
                ValidateExpression();
            }
        }

        private void ValidateExpression()
        {
            Expression.Validate();

            var totalConditions = Expression.ConditionsSum();
            if (totalConditions == 0)
            {
                throw new BadRequestException(BadRequestException.MISSING_MANDATORY_ARGUMENT_IN_PROPERTY, "expression", "KalturaUserSessionCondition");
            }

            if (totalConditions > MAX_CONDITIONS)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "expression's conditions", MAX_CONDITIONS);
            }
        }
    }

    public partial class KalturaUserSessionProfileListResponse : KalturaListResponse
    {
        /// <summary>
        /// A list of KalturaUserSessionProfile
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty("objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaUserSessionProfile> Objects { get; set; }
    }
}