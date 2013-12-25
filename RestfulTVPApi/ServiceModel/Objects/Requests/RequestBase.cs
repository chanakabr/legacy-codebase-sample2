using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack;
using ServiceStack.Api.Swagger;
using ServiceStack.ServiceHost;
using TVPApi;

namespace RestfulTVPApi.ServiceModel
{

    public abstract class RequestBase
    {
        [ApiMember(Name = "fields", Description = "Fields", ParameterType = "query", DataType = SwaggerType.String, IsRequired = false)]
        public string fields { get; set; }

        [ApiMember(Name = "X-InitObj", Description = "Initialization Object", ParameterType = "header", DataType = SwaggerType.String, IsRequired = true)]
        public InitializationObject InitObj { get; set; }
    }

    public abstract class PagingRequest : RequestBase
    {
        private int _page_number = 0;
        private int _page_size = 10;

        [ApiMember(Name = "page_number", Description = "Page Number", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = false)]
        public int page_number
        {
            get
            {
                return _page_number;
            }
            set
            {
                _page_number = value;
            }
        }

        [ApiMember(Name = "page_size", Description = "Page Size", ParameterType = "query", DataType = SwaggerType.Int, IsRequired = false)]
        public int page_size
        {
            get
            {
                return _page_size;
            }
            set
            {
                _page_size = value;
            }
        }
    }

}