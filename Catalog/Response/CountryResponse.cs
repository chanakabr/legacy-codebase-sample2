using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using GroupsCacheManager;
using Catalog.ws_users;
using ApiObjects.Response;

namespace Catalog.Response
{
    [DataContract]
    public class CountryResponse : BaseResponse
    {
        [DataMember]
        public Country Country { get; set; }

        [DataMember]
        public ApiObjects.Response.Status Status;

        public CountryResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            this.m_lObj = new List<BaseObject>();
        }
    }
}
