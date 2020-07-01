using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using ApiObjects.Base;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaChannelFilter : KalturaAssetFilter
    {

        private bool shouldUseChannelDefault = true;

        /// <summary>
        ///Channel Id
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        ///  /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restricted to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// user_interests - only valid value is "true". When enabled, only assets that the user defined as his interests (by tags and metas) will return.
        /// epg_channel_id – the channel identifier of the EPG program. *****Deprecated, please use linear_media_id instead*****
        /// linear_media_id – the linear media identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "not_entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// asset_type - valid values: "media", "epg", "recording" or any number that represents media type in group.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). 
        /// For alpha-numerical fields =, != (not), ~ (like), !~, ^ (any word starts with), ^= (phrase starts with), + (exists), !+ (not exists).
        /// Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each for the next operators: ~, !~, ^, ^=
        /// (maximum length of entire filter is 4096 characters)]]>
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }

        /// <summary>
        /// Exclude watched asset. 
        /// </summary>
        [DataMember(Name = "excludeWatched")]
        [JsonProperty("excludeWatched")]
        [XmlElement(ElementName = "excludeWatched", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public bool ExcludeWatched { get; set; }

        /// <summary>
        /// order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaAssetOrderBy OrderBy
        {
            get { return base.OrderBy; }
            set
            {
                base.OrderBy = value;
                shouldUseChannelDefault = false;
            }
        }

        public bool GetShouldUseChannelDefault()
        {
            if (DynamicOrderBy != null)
            {
                return false;
            }
            return shouldUseChannelDefault;
        }

        // Returns assets that belong to a channel
        internal virtual KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            KalturaAssetListResponse response = null;

            // TODO SHIR - GROUP_BY
            int domainId = (int)(contextData.DomainId ?? 0);
            if (this.ExcludeWatched)
            {
                if (pager.getPageIndex() > 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "excludeWatched", "pageIndex");
                }

                if (!contextData.UserId.HasValue || contextData.UserId.Value == 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_USER_ID, "userId");
                }
                int userId = (int)contextData.UserId.Value;

                response = ClientsManager.CatalogClient().GetChannelAssetsExcludeWatched(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(),
                pager.PageSize, this.IdEqual, this.OrderBy, this.KSql, this.GetShouldUseChannelDefault(), this.DynamicOrderBy);
            }
            else
            {
                var userId = contextData.UserId.ToString();
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);
                response = ClientsManager.CatalogClient().GetChannelAssets(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(), 
                    pager.PageSize, this.IdEqual, this.OrderBy, this.KSql, this.GetShouldUseChannelDefault(), this.DynamicOrderBy, responseProfile, isAllowedToViewInactiveAssets);
            }

            return response;
        }
    }
}