using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaSocialCommentFilter : KalturaFilter<KalturaSocialCommentOrderBy>
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


        internal void validate()
        {
            if (AssetIdEqual == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSocialCommentFilter.assetIdEqual");
            }
            if (AssetTypeEqual == KalturaAssetType.recording)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialCommentFilter.assetTypeEqual");
            }
            if (AssetTypeEqual == KalturaAssetType.epg && SocialPlatformEqual != KalturaSocialPlatform.IN_APP)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSocialCommentFilter.assetTypeEqual, KalturaSocialCommentFilter.socialPlatformEqual");
            }
        }

        public override KalturaSocialCommentOrderBy GetDefaultOrderByValue()
        {
            return KalturaSocialCommentOrderBy.CREATE_DATE_DESC;
        }
    }

    public enum KalturaSocialCommentOrderBy
    {
        CREATE_DATE_ASC,
        CREATE_DATE_DESC,
    }
}