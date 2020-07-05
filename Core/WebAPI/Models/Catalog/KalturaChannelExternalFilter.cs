using ApiObjects.Base;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaChannelExternalFilter : KalturaAssetFilter
    {
        /// <summary>
        ///External Channel Id. 
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }
        
        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual")]
        [SchemeProperty(MinFloat = -12, MaxFloat = 12)]
        public float UtcOffsetEqual { get; set; }

        /// <summary>
        ///FreeTextEqual
        /// </summary>
        [DataMember(Name = "freeText")]
        [JsonProperty("freeText")]
        [XmlElement(ElementName = "freeText", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string FreeText { get; set; }

        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            string deviceType = System.Web.HttpContext.Current.Request.GetUserAgentString();
            int domainId = (int)(contextData.DomainId ?? 0);
            var response = ClientsManager.CatalogClient().GetExternalChannelAssets(contextData.GroupId, this.IdEqual.ToString(), contextData.UserId.ToString(), domainId, contextData.Udid,
                contextData.Language, pager.getPageIndex(), pager.PageSize, this.OrderBy, deviceType, this.UtcOffsetEqual.ToString(), this.FreeText, this.DynamicOrderBy);
            return response;
        }
    }
}