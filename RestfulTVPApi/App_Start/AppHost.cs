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
using ServiceStack.ServiceModel.Serialization;
using System.Diagnostics;

namespace RestfulTVPApi
{
	public class AppHost : AppHostBase
	{		
		public AppHost() : base("RestAPI", new System.Reflection.Assembly[] { typeof(RequestBase).Assembly }) { }

		public override void Configure(Funq.Container container)
		{
			ServiceStack.Text.JsConfig.EmitCamelCaseNames = true;
            ServiceStack.Text.JsConfig.DateHandler = JsonDateHandler.ISO8601;
            ServiceStack.Text.JsConfig.ExcludeTypeInfo = true;

            container.Adapter = new SimpleInjectorAdapter();

            Plugins.Add(new SwaggerFeature());

            SetConfig(new EndpointHostConfig
            {
                //EnableFeatures = Feature.Json | Feature.Xml,
                //WriteErrorsToResponse = false,
                //DefaultContentType = ContentType.Json,
                CustomHttpHandlers = { { HttpStatusCode.NotFound, new CustomNotFoundHttpHandler() } },
            });

            //Exception outside of services
            this.ExceptionHandler = (IHttpRequest httpReq, IHttpResponse httpRes, string operationName, Exception ex) =>
            {
                HttpError httpError = null;

                if (ex is HttpError)
                {
                    httpError = new HttpError(new ResponseStatus("HttpException", ex.Message), ((HttpError)ex).StatusCode, ((HttpError)ex).StatusCode.ToString(), string.Empty);
                }
                else if (ex is RequestBindingException)
                {
                    httpError = new HttpError(new ResponseStatus("RequestBindingException", ex.InnerException.InnerException.Message), HttpStatusCode.BadRequest, HttpStatusCode.BadRequest.ToString(), string.Empty);
                }
                else
                {
                    httpError = new HttpError(new ResponseStatus(ex.GetType().ToTypeString(), "Unexpected Error."), HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), string.Empty);
                }

                ServiceStack.WebHost.Endpoints.Extensions.HttpResponseExtensions.WriteToResponse‌(httpRes, httpReq, httpError);
            };

            //Exception inside of services
            this.ServiceExceptionHandler = (httpReq, request, ex) =>
            {

                HttpError httpError = null;

                if (ex is UnknownGroupException)
                {
                    httpError = new HttpError(new ResponseStatus("UnknownGroupException", "Please check X-Init-Object."), HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized.ToString(), string.Empty);
                }
                else
                {
                    httpError = new HttpError(new ResponseStatus(ex.GetType().ToTypeString(), "Unexpected Error."), HttpStatusCode.InternalServerError, HttpStatusCode.InternalServerError.ToString(), string.Empty);
                }

                return httpError;
            };
		}

        public override IServiceRunner<TRequest> CreateServiceRunner<TRequest>(ActionContext actionContext)
        {
            return new CustomServiceRunner<TRequest>(this, actionContext);
        }

		public static void Start()
		{
			new AppHost().Init();
		}
	}
}
