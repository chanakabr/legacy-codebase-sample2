using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Web;
using CS_threescale;
using ServiceStack.Common.Web;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using TVPApi;
using RestfulTVPApi.ServiceInterface;

namespace RestfulTVPApi.ServiceInterface
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class RequiresAuthenticationAttribute : RequestFilterAttribute
    {
        public RequiresAuthenticationAttribute(ApplyTo applyTo)
        {
            this.ApplyTo = applyTo;
            this.Priority = (int) RequestFilterPriority.Authenticate;
        }

        public RequiresAuthenticationAttribute() : this(ApplyTo.All) {}

        public override void Execute(IHttpRequest httpReq, IHttpResponse httpRes, object reqDto)
        {
            try
            {
                IApi _3ScaleAPI = new Api(ConfigurationManager.AppSettings["3SCALE_PROVIDER_KEY"]);

                Hashtable parameters = new Hashtable();

                if (httpReq.Headers["X-App-Id"] == null || httpReq.Headers["X-App-Key"] == null)
                {
                    throw new HttpError(HttpStatusCode.Unauthorized, "Credentials not authorized");
                }

                parameters.Add("app_id", httpReq.Headers["X-App-Id"]);
                parameters.Add("app_key", httpReq.Headers["X-App-Key"]);
                
                Hashtable usage = new Hashtable();

                usage.Add(reqDto.GetType().Name, "1");

                parameters.Add("usage", usage);

                AuthorizeResponse resp = _3ScaleAPI.authrep(parameters);

                if (!resp.authorized)
                {
                    if (resp.reason.StartsWith("application key") && resp.reason.EndsWith("is invalid"))
                    {
                        throw new HttpError(HttpStatusCode.Unauthorized, "Credentials not authorized");
                    }
                    else
                    {
                        throw new HttpError(HttpStatusCode.Unauthorized, resp.reason);
                    }
                }
            }
            catch (ApiException ex)
            {
                if (ex.Message.Contains("application_not_found"))
                {
                    throw new HttpError(HttpStatusCode.Unauthorized, "Credentials not authorized");
                }
                if (ex.Message.Contains("metric_invalid"))
                {

                }
                else
                {
                    throw ex;   
                }
            }

        }
    }
}