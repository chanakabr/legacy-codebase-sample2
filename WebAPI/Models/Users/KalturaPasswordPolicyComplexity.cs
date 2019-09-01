using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// Password policy complexity
    /// </summary>
    public abstract partial class KalturaPasswordPolicyComplexity : KalturaOTTObject
    {
        /// <summary>
        /// Minimum number of characters which the compexity is applied on.
        /// </summary>
        [DataMember(Name = "minimumAppliedCharactersNumber")]
        [JsonProperty("minimumAppliedCharactersNumber")]
        [XmlElement(ElementName = "minimumAppliedCharactersNumber")]
        [SchemeInput(MinInteger = 1)]
        public int MinimumAppliedCharactersNumber { get; set; }

        /// <summary>
        /// Description for return error when validating the password.
        /// </summary>
        [DataMember(Name = "errorValidationDescription")]
        [JsonProperty("errorValidationDescription")]
        [XmlElement(ElementName = "errorValidationDescription")]
        public string ErrorValidationDescription { get; set; }

        internal abstract bool ValidatePasswordComplexity(string password);
    }

    public partial class KalturaUpperCaseComplexity : KalturaPasswordPolicyComplexity
    {
        const string REGEX_UPPERCASE = @"[A-Z]";

        internal override bool ValidatePasswordComplexity(string password)
        {
            var match = Regex.Match(password, REGEX_UPPERCASE);
            if (match.Success && match.Groups.Count >= MinimumAppliedCharactersNumber)
            {
                return true;
            }

            return false;
        }
    }
}