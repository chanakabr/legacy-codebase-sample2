using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [SchemeClass(OneOf = new[] {"idIn", "mediaFileTypeKeyName"})]
    public partial class KalturaMediaFileDynamicDataFilter : KalturaFilter<KalturaMediaFileDynamicDataOrderBy>
    {
        /// <summary>
        /// A comma-separated list of KalturaMediaFileDynamicData.Id to be searched.
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement("idIn")]
        [SchemeProperty(IsNullable = true, DynamicMinInt = 1, MinLength = 1, MaxLength = 200)]
        public string IdIn { get; set; }

        /// <summary>
        /// An integer representing the the mediaFileType holding the keys for which the values should be stored.
        /// </summary>
        [DataMember(Name = "mediaFileTypeId")]
        [JsonProperty("mediaFileTypeId")]
        [XmlElement("mediaFileTypeId")]
        [SchemeProperty(IsNullable = true, MinLong = 1)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public long? MediaFileTypeId { get; set; }

        /// <summary>
        /// A string representing the key name within the mediaFileType that identifies the list corresponding
        /// to that key name.
        /// </summary>
        [DataMember(Name = "mediaFileTypeKeyName")]
        [JsonProperty("mediaFileTypeKeyName")]
        [XmlElement("mediaFileTypeKeyName")]
        [SchemeProperty(IsNullable = true, MinLength = 1, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COMMAS_PATTERN)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string MediaFileTypeKeyName { get; set; }

        /// <summary>
        /// A string representing a specific value to be searched.
        /// </summary>
        [DataMember(Name = "valueEqual")]
        [JsonProperty("valueEqual")]
        [XmlElement("valueEqual")]
        [SchemeProperty(IsNullable = true, MinLength = 0, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COMMAS_PATTERN)]
        public string ValueEqual { get; set; }

        /// <summary>
        /// A string representing the beginning of multiple (zero or more) matching values.
        /// </summary>
        [DataMember(Name = "valueStartsWith")]
        [JsonProperty("valueStartsWith")]
        [XmlElement("valueStartsWith")]
        [SchemeProperty(IsNullable = true, MinLength = 0, MaxLength = 50, Pattern = SchemeInputAttribute.NO_COMMAS_PATTERN)]
        public string ValueStartsWith { get; set; }

        public override KalturaMediaFileDynamicDataOrderBy GetDefaultOrderByValue()
        {
            return KalturaMediaFileDynamicDataOrderBy.NONE;
        }
    }
}
