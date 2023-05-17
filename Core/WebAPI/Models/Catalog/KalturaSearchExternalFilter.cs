using ApiObjects.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Extensions;

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
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSearchExternalFilter.typeIn");
                }
            }

            return values;
        }

        // Search for assets via external service (e.g. external recommendation engine). 
        //Search can return multi asset types. Support on-demand, per asset enrichment. Maximum number of returned assets – 100, using paging
        internal override KalturaAssetListResponse GetAssets(ContextData contextData, KalturaBaseResponseProfile responseProfile, KalturaFilterPager pager)
        {
            var typeIn = getTypeIn();
            return ClientsManager.CatalogClient().GetSearchMediaExternal(contextData, pager.GetRealPageIndex(), pager.PageSize, Query, typeIn, UtcOffsetEqual);
        }
    }
}
