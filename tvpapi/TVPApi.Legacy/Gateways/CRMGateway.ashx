<%@ WebHandler Language="C#" Class="CRMGateway" %>

using System;
using System.Web;
using TVPApi;
using System.Reflection;
using TVPApiServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class CRMGateway : IHttpHandler
{
    private static readonly KLogMonitor.KLogger logger = new KLogMonitor.KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

    public void ProcessRequest(HttpContext context)
    {

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.AppendHeader("Access-Control-Allow-Origin", "*");

        object response = new CRMResponse();

        if (context.Request.IsSecureConnection)
        {
            string sApiUser;
            string sApiPass;

            try
            {
                if (ConnectionHelper.GetApiCredentials(context.Request.Headers["X-CRM-USER"], context.Request.Headers["X-CRM-PASS"], out sApiUser, out sApiPass))
                {
                    string methodName = HttpContext.Current.Request.QueryString["m"];

                    if (!string.IsNullOrEmpty(methodName))
                    {
                        CRMService service = new CRMService(sApiUser, sApiPass);

                        MethodInfo methodInfo = service.GetType().GetMethod(methodName);

                        if (methodInfo != null)
                        {
                            List<object> methodParameters = new List<object>();

                            ParameterInfo[] parametersInfo = methodInfo.GetParameters();

                            if (parametersInfo.Length > 0)
                            {
                                Stream body = HttpContext.Current.Request.InputStream;
                                Encoding encoding = HttpContext.Current.Request.ContentEncoding;

                                using (StreamReader reader = new System.IO.StreamReader(body, encoding))
                                {
                                    string sJsonRequest = reader.ReadToEnd();

                                    object methodParameter = JsonConvert.DeserializeObject(sJsonRequest, parametersInfo[0].ParameterType);

                                    methodParameters.Add(methodParameter);
                                }
                            }

                            response = methodInfo.Invoke(service, methodParameters.ToArray());
                        }
                        else
                        {
                            ((CRMResponse)response).status_code = TVPApiModule.Objects.CRM.CRMResponseStatus.MethodNotFound;
                        }
                    }
                    else
                    {
                        ((CRMResponse)response).status_code = TVPApiModule.Objects.CRM.CRMResponseStatus.MethodNotIncludedInQueryString;
                    }
                }
                else
                {
                    ((CRMResponse)response).status_code = TVPApiModule.Objects.CRM.CRMResponseStatus.CredentialsNotAuthorized;
                }

            }
            catch (JsonSerializationException ex)
            {
                logger.ErrorFormat("JsonSerializationException Exception, Error Message: {0}", ex.Message);

                ((CRMResponse)response).status_code = TVPApiModule.Objects.CRM.CRMResponseStatus.SerializationError;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("CRMGateway Exception, Error Message: {0}", ex.Message);

                ((CRMResponse)response).status_code = TVPApiModule.Objects.CRM.CRMResponseStatus.UnexpectedError;
            }
        }
        else
        {
            ((CRMResponse)response).status_code = TVPApiModule.Objects.CRM.CRMResponseStatus.OnlySecureConnectionAllowed;
        }

        string sResponse = JsonConvert.SerializeObject(response);

        HttpContext.Current.Response.Write(sResponse);
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}

