using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;

namespace WebAPI.Models.ConditionalAccess.FilterActions.Files
{
    /// <summary>
    /// FilterFile By AudioCodec
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "audioCodecIn" })]
    public abstract partial class KalturaFilterFileByAudioCodecAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated audioCodecs
        /// </summary>
        [DataMember(Name = "audioCodecIn")]
        [JsonProperty("audioCodecIn")]
        [XmlElement(ElementName = "audioCodecIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string AudioCodecIn { get; set; }
    }
}