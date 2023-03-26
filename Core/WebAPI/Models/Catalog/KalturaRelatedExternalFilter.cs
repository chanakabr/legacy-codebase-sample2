using ApiObjects.Base;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaRelatedExternalFilter : KalturaAssetFilter
    {
         /// <summary>
        /// the External ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

         /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }        

        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual")]
        public int UtcOffsetEqual { get; set; }

         /// <summary>
        ///FreeText
        /// </summary>
        [DataMember(Name = "freeText")]
        [JsonProperty("freeText")]
        [XmlElement(ElementName = "freeText", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string FreeText { get; set; }
  
        internal List<int> getTypeIn()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(TypeIn, "KalturaRelatedExternalFilter.typeIn");
        }

        //Return list of assets that are related to a provided asset ID. Returned assets can be within multi asset types or be of same type as the provided asset. 
        //Support on-demand, per asset enrichment. Related assets are provided from the external source (e.g. external recommendation engine). 
        //Maximum number of returned assets – 20, using paging  
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            return ClientsManager.CatalogClient().GetRelatedMediaExternal
                (contextData, pager.GetRealPageIndex(), pager.PageSize, IdEqual, getTypeIn(), UtcOffsetEqual, FreeText);
        }
    }
}