using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Core.Catalog.Response;

namespace Core.Catalog.Response
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
            status = new ApiObjects.Response.Status();
        }
    }
}