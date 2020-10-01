using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAssetFilter : KalturaPersistedFilter<KalturaAssetOrderBy>
    {
        public override KalturaAssetOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.RELEVANCY_DESC;
        }

        internal virtual void Validate()
        {   
        }

        /// <summary>
        /// dynamicOrderBy - order by Meta
        /// </summary>
        [DataMember(Name = "dynamicOrderBy")]
        [JsonProperty("dynamicOrderBy")]
        [XmlElement(ElementName = "dynamicOrderBy", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public KalturaDynamicOrderBy DynamicOrderBy { get; set; }

        internal virtual KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            // TODO refactoring. duplicate with KalturaSearchAssetFilter
            var userId = contextData.UserId.ToString();
            var domainId = (int)(contextData.DomainId ?? 0);
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(contextData.GroupId, userId, true);

            var response = ClientsManager.CatalogClient().SearchAssets(
                contextData.GroupId, 
                userId,
                domainId,
                contextData.Udid,
                contextData.Language,
                pager.getPageIndex(),
                pager.PageSize, 
                null,
                OrderBy, 
                null, 
                null,
                contextData.ManagementData,
                DynamicOrderBy,
                null,
                responseProfile,
                isAllowedToViewInactiveAssets,
                null);

            return response;
        }
    }
}