using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using AdapterControllers;

namespace Catalog.Response
{
    /// <summary>
    /// Catalog response that holds list of external results and their types
    /// </summary>
    [DataContract]
    class MediaIdsStatusResponse : MediaIdsResponse
    {
        [DataMember]
        public ApiObjects.Response.Status Status;

        public MediaIdsStatusResponse() : base()
        {
            Status = new ApiObjects.Response.Status();
        }
    }
}
