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
    public partial class KalturaChannelFilter : KalturaBaseSearchAssetFilter
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
                pager.PageSize, this.IdEqual, this.OrderBy, this.Ksql, this.GetShouldUseChannelDefault(), this.DynamicOrderBy);
            }
            else
            {
                var userId = contextData.UserId.ToString();
                bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);
                response = ClientsManager.CatalogClient().GetChannelAssets(contextData.GroupId, userId, domainId, contextData.Udid, contextData.Language, pager.getPageIndex(), 
                    pager.PageSize, this.IdEqual, this.OrderBy, this.Ksql, this.GetShouldUseChannelDefault(), this.DynamicOrderBy, responseProfile, isAllowedToViewInactiveAssets, this.getGroupByValue());
            }

            return response;
        }
    }
}