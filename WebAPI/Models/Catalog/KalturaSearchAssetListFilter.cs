using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaSearchAssetListFilter : KalturaSearchAssetFilter
    {
        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }
    }
}