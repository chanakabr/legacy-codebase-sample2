using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Catalog
{
    /// <summary>
    /// Catalog response that holds list of search results and their types
    /// </summary>
    [DataContract]
    public class WatchHistoryResponse : BaseResponse
    {
        /// <summary>
        /// List of unified search results: id, type, update date
        /// </summary>
        [DataMember]
        public List<UnifiedSearchResult> searchResults;

        [DataMember]
        public ApiObjects.Response.Status status;

        public WatchHistoryResponse()
        {
            searchResults = new List<UnifiedSearchResult>();
            status = new ApiObjects.Response.Status();
        }
    }
}