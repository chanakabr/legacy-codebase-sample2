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
    /// FilterFile By FileType
    /// </summary>
    [Serializable]
    [SchemeClass(Required = new string[] { "fileTypeIdIn" })]
    public abstract partial class KalturaFilterFileByFileTypeIdAction : KalturaFilterAction
    {
        /// <summary>
        /// List of comma separated fileTypesIds
        /// </summary>
        [DataMember(Name = "fileTypeIdIn")]
        [JsonProperty("fileTypeIdIn")]
        [XmlElement(ElementName = "fileTypeIdIn")]
        [SchemeProperty(MinLength = 1, Pattern = SchemePropertyAttribute.NOT_EMPTY_PATTERN)]
        public string FileTypeIdIn { get; set; }
    }
}