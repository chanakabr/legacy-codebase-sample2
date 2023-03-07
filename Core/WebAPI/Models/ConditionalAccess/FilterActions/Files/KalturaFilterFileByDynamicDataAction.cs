using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// Filter File By Dynamic Data
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new[] { "key", "values" })]
    public abstract partial class KalturaFilterFileByDynamicDataAction : KalturaFilterAction
    {
        /// <summary>
        /// Key to be searched
        /// </summary>
        [DataMember(Name = "key")]
        [JsonProperty("key")]
        [XmlElement(ElementName = "key")]
        [SchemeProperty(MinLength = 1, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COMMAS_PATTERN)]
        public string Key { get; set; }

        /// <summary>
        /// Comma separated values to be searched
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty("values")]
        [XmlElement(ElementName = "values")]
        [SchemeProperty(MinLength = 1, MaxLength = 200, Pattern = SchemeInputAttribute.NOT_EMPTY_PATTERN)]
        public string Values { get; set; }
    }
}