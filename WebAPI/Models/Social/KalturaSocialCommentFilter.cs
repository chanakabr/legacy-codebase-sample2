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
        /// Comma separated list of social platforms to filter by
        /// </summary>
        [DataMember(Name = "socialPlatfomIn")]
        [JsonProperty("socialPlatfomIn")]
        [XmlElement(ElementName = "socialPlatfomIn")]
        public string SocialPlatfomIn { get; set; }

        /// <summary>
        /// The create date from which to get the comments 
        /// </summary>
        [DataMember(Name = "createDateGreaterThan")]
        [JsonProperty("createDateGreaterThan")]
        [XmlElement(ElementName = "createDateGreaterThan")]
        public long CreateDateGreaterThan { get; set; }

        private List<KalturaSocialPlatform> socialPlatfomIn;

        public List<KalturaSocialPlatform> GetSocialPlatfomIn()
        {
            if (socialPlatfomIn != null)
            {
                return socialPlatfomIn;
            }

            List<KalturaSocialPlatform> socialPlatforms = new List<KalturaSocialPlatform>();
            if (!string.IsNullOrEmpty(SocialPlatfomIn))
            {
                string[] splitPlatforms = SocialPlatfomIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string platform in splitPlatforms)
                {
                    KalturaSocialPlatform parsedPlatform;
                    if (Enum.TryParse(platform, true, out parsedPlatform))
                    {
                        socialPlatforms.Add(parsedPlatform);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialCommentFilter.socialPlatfomIn", platform);
                    }
                }
            }

            socialPlatfomIn = socialPlatforms;
            return socialPlatforms;

        }

        internal void validate()
        {
            GetSocialPlatfomIn();

            if (AssetIdEqual == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSocialCommentFilter.assetIdEqual");
            }
            if (AssetTypeEqual == KalturaAssetType.recording)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialCommentFilter.assetTypeEqual");
            }
            if (AssetTypeEqual == KalturaAssetType.epg && (socialPlatfomIn.Count > 1 || (socialPlatfomIn.Count > 0 && socialPlatfomIn[0] != KalturaSocialPlatform.IN_APP)))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaSocialCommentFilter.assetTypeEqual, KalturaSocialCommentFilter.socialPlatfomIn");
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