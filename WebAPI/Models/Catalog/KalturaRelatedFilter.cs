using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaRelatedFilter : KalturaBaseSearchAssetFilter
    {
        /// <summary>
        /// the ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        [SchemeProperty(MinInteger = 1)]
        public int? IdEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – same type as the provided asset.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }

        public int getMediaId()
        {
            return IdEqual.Value;
        }

        internal List<int> getTypeIn()
        {
            return this.GetItemsIn<List<int>, int>(TypeIn, "KalturaRelatedFilter.typeIn");
        }

        internal override void Validate()
        {
            if (!IdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaRelatedFilter.IdEqual");
            }
        }
    }
}