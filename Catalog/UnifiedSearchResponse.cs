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
    public class UnifiedSearchResponse : BaseResponse
    {
        /// <summary>
        /// List of unified search results: id, type, update date
        /// </summary>
        [DataMember]
        public List<UnifiedSearchResult> searchResults;
        
        public UnifiedSearchResponse()
        {
            searchResults = new List<UnifiedSearchResult>();
        }
    }
}