using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public partial class KalturaSocialCommentFilter : KalturaFilter<KalturaSocialCommentOrderBy>
    {
        /// <summary>
        /// Asset ID to filter by
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long AssetIdEqual { get; set; }

        /// <summary>
        /// Asset type to filter by, currently only VOD (media)
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType AssetTypeEqual { get; set; }

        /// <summary>
        /// Comma separated list of social actions to filter by
        /// </summary>
        [DataMember(Name = "socialPlatformEqual")]
        [JsonProperty("socialPlatformEqual")]
        [XmlElement(ElementName = "socialPlatformEqual")]
        public KalturaSocialPlatform SocialPlatformEqual { get; set; }

        /// <summary>
        /// The create date from which to get the comments 
        /// </summary>
        [DataMember(Name = "createDateGreaterThan")]
        [JsonProperty("createDateGreaterThan")]
        [XmlElement(ElementName = "createDateGreaterThan")]
        public long CreateDateGreaterThan { get; set; }

        public override KalturaSocialCommentOrderBy GetDefaultOrderByValue()
        {
            return KalturaSocialCommentOrderBy.CREATE_DATE_DESC;
        }
    }
}