using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering external recordings
    /// </summary>
    [Serializable]
    public partial class KalturaExternalRecordingFilter : KalturaRecordingFilter
    {
        /// <summary>
        /// MetaData filtering 
        /// </summary>
        [DataMember(Name = "metaData")]
        [JsonProperty("metaData")]
        [XmlElement(ElementName = "metaData", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public SerializableDictionary<string, KalturaStringValue> MetaData { get; set; }

        new internal void Validate()
        {
            base.Validate();

            if (MetaData == null)
            {
                MetaData = new SerializableDictionary<string, KalturaStringValue>();
            }
        }
    }
}