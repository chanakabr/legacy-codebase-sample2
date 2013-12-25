using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using ServiceStack.Configuration;
using ServiceStack.OrmLite;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.ServiceInterface.ServiceModel;
using ServiceStack.WebHost.Endpoints;
using TVPApi;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RestfulTVPApi.ServiceInterface;
using System.Web;
using System.Net;
using ServiceStack.ServiceHost;
using ServiceStack.Common.Web;
using ServiceStack.Api.Swagger;
using ServiceStack.Text;
using RestfulTVPApi.ServiceModel;

namespace RestfulTVPApi
{
	public class AppHost : AppHostBase
	{		
		public AppHost() //Tell ServiceStack the name and where to find your web services
            : base("StarterTemplate ASP.NET Host", new System.Reflection.Assembly[] { typeof(RequestBase).Assembly }) { }

		public override void Configure(Funq.Container container)
		{
			ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.DateHandler = JsonDateHandler.ISO8601;
            ServiceStack.Text.JsConfig.ExcludeTypeInfo = true;

            container.Adapter = new SimpleInjectorAdapter();

            Plugins.Add(new SwaggerFeature());

            //Exception outside of services
            this.ExceptionHandler = (IHttpRequest httpReq, IHttpResponse httpRes, string operationName, Exception ex) =>
            {
                HttpError dto = null;

                if (ex is HttpError)
                {
                    dto = new HttpError(new ResponseStatus("HttpException", ex.Message), ((HttpError)ex).StatusCode, ((HttpError)ex).StatusCode.ToString(), string.Empty);
                }
                else
                {
                    dto = new HttpError(new ResponseStatus(ex.GetType().ToTypeString(), ex.Message), HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), string.Empty);
                }

                ServiceStack.WebHost.Endpoints.Extensions.HttpResponseExtensions.WriteToResponse‌(httpRes, httpReq, dto);
            };

            //Exception inside of services
            this.ServiceExceptionHandler = (httpReq, request, ex) => {

                HttpError dto = null;

                if (ex is UnknownGroupException)
                {
                    dto = new HttpError(new ResponseStatus("UnknownGroupException", ex.Message), HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString(), string.Empty);
                }
                else
                {
                    dto = new HttpError(new ResponseStatus(ex.GetType().ToTypeString(), ex.Message), HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), string.Empty);
                }

                return dto;
            };
		}

		public static void Start()
		{
			new AppHost().Init();
		}
	}
}
