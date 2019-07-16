using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Filtering cloud external recordings
    /// </summary>
    [Serializable]
    public partial class KalturaCloudRecordingFilter : KalturaExternalRecordingFilter
    {
        /// <summary>
        /// Adapter Data
        /// </summary>
        [DataMember(Name = "adapterData")]
        [JsonProperty(PropertyName = "adapterData")]
        [XmlArray(ElementName = "adapterData", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string AdapterData { get; set; }

        new internal void Validate()
        {
            try
            {
                var json = JObject.Parse(AdapterData);
            }
            catch
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaCloudRecordingFilter.AdapterData");
            }
        }
    }
}