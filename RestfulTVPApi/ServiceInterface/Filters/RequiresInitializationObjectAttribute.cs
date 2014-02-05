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
using RestfulTVPApi.ServiceModel;
using TVPApiModule.Objects;

namespace RestfulTVPApi.ServiceInterface
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class RequiresInitializationObjectAttribute : RequestFilterAttribute
    {
        public RequiresInitializationObjectAttribute(ApplyTo applyTo)
        {
            this.ApplyTo = applyTo;
            this.Priority = (int) RequestFilterPriority.RequiredPermission;
        }

        public RequiresInitializationObjectAttribute()
            : this(ApplyTo.All) {}

        public override void Execute(IHttpRequest httpReq, IHttpResponse httpRes, object reqDto)
        {
            try
            {
                RequestBase BaseRequest = (RequestBase)reqDto;

                string sInitObj = httpReq.Headers.GetValues("X-Init-Object").FirstOrDefault();

                byte[] bytes = Convert.FromBase64String(sInitObj);

                using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
                {
                    ms.Write(bytes, 0, bytes.Length);

                    ms.Position = 0;

                    BaseRequest.InitObj = (InitializationObject)new BinaryFormatter().Deserialize(ms);
                }
            }
            catch (Exception)
            {
                throw new HttpError(HttpStatusCode.BadRequest, "Invalid Token");
            }
        }
    }
}