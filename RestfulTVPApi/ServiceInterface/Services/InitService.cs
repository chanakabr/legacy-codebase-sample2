using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using System.Net;
using ServiceStack.Api.Swagger;
using ServiceStack.Common.Web;
using ServiceStack.PartialResponse.ServiceModel;
using System.Collections.Generic;
using RestfulTVPApi.ServiceModel;
using System.Linq;
using TVPApi;
using ServiceStack;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/init", "POST", Summary = "Get Secured Initialization Object", Notes = "Get Secured Initialization Object")]
    public class SecuredInitObjRequest : IReturn<string>
    {
        [ApiMember(Name = "init_obj", Description = "Initialization Object", ParameterType = "body", DataType = "InitializationObject", IsRequired = true)]
        public InitializationObject init_obj { get; set; }
    }

    #endregion

    [RequiresAuthentication]
    public class InitService : Service
    {
        public HttpResult Post(SecuredInitObjRequest request)
        {
            if (request.init_obj == null)
            {
                return new HttpResult(HttpStatusCode.BadRequest);
            }

            string _token = string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, request.init_obj);

                _token = Convert.ToBase64String(ms.ToArray());
            }

            return new HttpResult(_token, HttpStatusCode.OK);
        }
    }
}
