using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Upload
{
    /// <summary>
    /// indicates the DynamicList object type in the bulk file
    /// </summary>
    public abstract partial class KalturaBulkUploadDynamicListData : KalturaBulkUploadObjectData
    {
        /// <summary>
        /// Identifies the dynamicList Id
        /// </summary>
        [DataMember(Name = "dynamicListId")]
        [JsonProperty(PropertyName = "dynamicListId")]
        [XmlElement(ElementName = "dynamicListId")]
        [SchemeProperty(MinLong = 1)]
        public long DynamicListId { get; set; }
    }
}
