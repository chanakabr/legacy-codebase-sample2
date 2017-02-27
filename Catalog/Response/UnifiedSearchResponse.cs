using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Catalog.Response;

namespace Catalog.Response
{
    /// <summary>
    /// Catalog response that holds list of search results and their types
    /// </summary>
    [DataContract]
    public class UnifiedSearchResponse : BaseResponse
    {
        /// <summary>
        /// List of unified search results: id, type, update date
        /// </summary>
        [DataMember]
        public List<UnifiedSearchResult> searchResults;

        [DataMember]
        public List<AggregationsResult> aggregationResults;

        [DataMember]
        public ApiObjects.Response.Status status;

        [DataMember]
        public string requestId;

        /// <summary>
        /// Last search document offset/index the response contains
        /// </summary>
        [DataMember]
        public int to;

        public UnifiedSearchResponse()
        {
            searchResults = new List<UnifiedSearchResult>();
            aggregationResults = new List<AggregationsResult>();
            status = new ApiObjects.Response.Status();
        }
    }
}