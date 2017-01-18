using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects.MediaMarks;
using Core.Catalog.Response;

namespace Core.Catalog.Response
{
    /// <summary>
    /// Catalog response that holds list of search results and their types
    /// </summary>
    [DataContract]
    public class WatchHistoryResponse : BaseResponse
    {
        [DataMember]
        public List<UserWatchHistory> result;

        [DataMember]
        public ApiObjects.Response.Status status;

        public WatchHistoryResponse()
        {
            result = new List<UserWatchHistory>();
            status = new ApiObjects.Response.Status();
        }
    }
}