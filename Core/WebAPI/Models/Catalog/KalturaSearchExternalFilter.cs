using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using ApiObjects.Base;
using WebAPI.ClientManagers.Client;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaSearchExternalFilter : KalturaAssetFilter
    {
        /// <summary>
        ///Query
        /// </summary>
        [DataMember(Name = "query")]
        [JsonProperty("query")]
        [XmlElement(ElementName = "query", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string Query { get; set; }

        /// <summary>
        /// UtcOffsetEqual 
        /// </summary>
        [DataMember(Name = "utcOffsetEqual")]
        [JsonProperty("utcOffsetEqual")]
        [XmlElement(ElementName = "utcOffsetEqual")]
        public int UtcOffsetEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn")]
        public string TypeIn { get; set; }

        internal List<int> getTypeIn()
        {
            if (string.IsNullOrEmpty(TypeIn))
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");

            List<int> values = new List<int>();
            string[] stringValues = TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            bool containsEpg = false;
            bool containsMedia = false;
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    if (value == 0)
                    {
                        containsEpg = true;
                    }
                    else
                    {
                        containsMedia = true;
                    }

                    if (containsEpg && containsMedia)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn can't contain both EPG and Media");
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");
                }
            }

            return values;
        }

        internal List<string> convertQueryToList()
        {
            return this.GetItemsIn<List<string>, string>(Query, "KalturaSearchExternalFilter.query");
        }

        // Search for assets via external service (e.g. external recommendation engine). 
        //Search can return multi asset types. Support on-demand, per asset enrichment. Maximum number of returned assets – 100, using paging
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var domainId = (int)(contextData.DomainId ?? 0);
            var typeIn = getTypeIn();
            var siteGuid = contextData.UserId.ToString();

            return typeIn.Contains(0)
                ? ClientsManager.CatalogClient().GetEPGByExternalIds(
                    contextData.GroupId,
                    siteGuid,
                    domainId,
                    contextData.Udid,
                    contextData.Language,
                    pager.getPageIndex(),
                    pager.PageSize,
                    convertQueryToList())
                : ClientsManager.CatalogClient().GetSearchMediaExternal(
                    contextData.GroupId,
                    siteGuid,
                    domainId,
                    contextData.Udid,
                    contextData.Language,
                    pager.getPageIndex(),
                    pager.PageSize,
                    Query,
                    typeIn,
                    UtcOffsetEqual);
        }
    }
}
