using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// FilterFile By Label
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "labelIn" })]
    public abstract partial class KalturaFilterFileByLabelAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated labels
        /// </summary>
        [DataMember(Name = "labelIn")]
        [JsonProperty("labelIn")]
        [XmlElement(ElementName = "labelIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string LabelIn { get; set; }
    }
}