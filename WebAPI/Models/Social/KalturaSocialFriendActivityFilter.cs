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
    public class KalturaSocialFriendActivityFilter : KalturaFilter<KalturaSocialFriendActivityOrderBy>
    {
        /// <summary>
        /// Asset ID to filter by
        /// </summary>
        [DataMember(Name = "assetIdEqual")]
        [JsonProperty("assetIdEqual")]
        [XmlElement(ElementName = "assetIdEqual")]
        public long? AssetIdEqual { get; set; }

        /// <summary>
        /// Asset type to filter by, currently only VOD (media)
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty("assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual")]
        public KalturaAssetType? AssetTypeEqual { get; set; }

        /// <summary>
        /// Comma separated list of social actions to filter by
        /// </summary>
        [DataMember(Name = "actionIn")]
        [JsonProperty("actionIn")]
        [XmlElement(ElementName = "actionIn")]
        public string ActionIn { get; set; }

        public List<KalturaSocialActionType> GetActionIn()
        {
            List<KalturaSocialActionType> actions = new List<KalturaSocialActionType>();
            if (!string.IsNullOrEmpty(ActionIn))
            {
                string[] splitActions = ActionIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string action in splitActions)
                {
                    KalturaSocialActionType parsedAction;
                    if (Enum.TryParse(action, true, out parsedAction))
                    {
                        actions.Add(parsedAction);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialFriendActivityFilter.actionIn", action);
                    }
                }
            }

            return actions;

        }

        internal void validate()
        {
            if (AssetIdEqual.HasValue && AssetIdEqual.Value > 0 && !AssetTypeEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSocialFriendActivityFilter.assetTypeEqual");
            }

            if (AssetTypeEqual.HasValue && AssetTypeEqual.Value != KalturaAssetType.media)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaSocialFriendActivityFilter.assetTypeEqual", AssetTypeEqual.Value);
            }
        }

        public override KalturaSocialFriendActivityOrderBy GetDefaultOrderByValue()
        {
            return KalturaSocialFriendActivityOrderBy.NONE;
        }
    }

    public enum KalturaSocialFriendActivityOrderBy
    {
        NONE
    }
}