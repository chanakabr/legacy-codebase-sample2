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
    /// Catalog response that holds list of external search results and their types
    /// </summary>
    [DataContract]
    public class UnifiedSearchExternalResponse : UnifiedSearchResponse
    {
        public UnifiedSearchExternalResponse()
            : base()
        {           
        }
    }
}