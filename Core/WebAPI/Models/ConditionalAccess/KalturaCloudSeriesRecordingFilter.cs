using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Cloud series filtering recordings
    /// </summary>
    [Serializable]
    public partial class KalturaCloudSeriesRecordingFilter : KalturaSeriesRecordingFilter
    {
        /// <summary>
        /// Adapter Data
        /// </summary>
        [DataMember(Name = "adapterData")]
        [JsonProperty(PropertyName = "adapterData")]
        [XmlArray(ElementName = "adapterData", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public SerializableDictionary<string, KalturaStringValue> AdapterData { get; set; }
    }
}