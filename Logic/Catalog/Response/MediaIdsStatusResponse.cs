using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AdapterControllers;

namespace Core.Catalog.Response
{
    /// <summary>
    /// Catalog response that holds list of external results and their types
    /// </summary>
    [DataContract]
    public class MediaIdsStatusResponse : MediaIdsResponse
    {
        [DataMember]
        public ApiObjects.Response.Status Status;
        
        [DataMember]
        public string RequestId;

        public MediaIdsStatusResponse() : base()
        {
            Status = new ApiObjects.Response.Status();
        }
    }
}
