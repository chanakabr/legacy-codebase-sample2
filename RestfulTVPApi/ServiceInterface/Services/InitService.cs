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
using TVPApiModule.Objects;

namespace RestfulTVPApi.ServiceInterface
{

    #region Objects

    [Route("/init", "POST", Summary = "Get Secured Initialization Object", Notes = "Get Secured Initialization Object")]
    public class SecuredInitObjRequest : IReturn<string>
    {
        [ApiMember(Name = "initObj", Description = "Initialization Object", ParameterType = "body", DataType = "InitializationObject", IsRequired = true)]
        public InitializationObject initObj { get; set; }
    }

    #endregion
    
    public class InitService : Service
    {
        public string Post(SecuredInitObjRequest request)
        {
            string _token = string.Empty;

            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, request.initObj);

                _token = Convert.ToBase64String(ms.ToArray());
            }

            return _token;
        }
    }
}
