using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using WebAPI.ClientManagers;
using WebAPI.Controllers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.Filters
{
    public class RequestParser : ActionFilterAttribute
    {
        public const string REQUEST_METHOD_PARAMETERS = "requestMethodParameters";
        public const string REQUEST_PARTNER_ID = "requestPartnerID";

        //public static string PartnerID
        //{
        //    get { return (string)HttpContext.Current.Items[REQUEST_PARTNER_ID]; }
        //}

        public static object GetRequestPayload()
        {
            return HttpContext.Current.Items[REQUEST_METHOD_PARAMETERS];
        }

        private void createMethodInvoker(string serviceName, string actionName, out MethodInfo methodInfo, out object classInstance)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            Type controller = asm.GetType(string.Format("WebAPI.Controllers.{0}Controller", serviceName), false, true);

            if (controller == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidService, "Service doesn't exist");

            methodInfo = controller.GetMethod(actionName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (methodInfo == null)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.InvalidAction, "Action doesn't exist");

            classInstance = Activator.CreateInstance(controller, null);
        }

        public async override void OnActionExecuting(HttpActionContext actionContext)
        {
            var rd = actionContext.ControllerContext.RouteData;
            string currentAction = rd.Values["action_name"].ToString();
            string currentController = rd.Values["service_name"].ToString();

            MethodInfo methodInfo = null;
            object classInstance = null;

            createMethodInvoker(currentController, currentAction, out methodInfo, out classInstance);

            if (actionContext.Request.Method == HttpMethod.Post)
            {
                string result = await actionContext.Request.Content.ReadAsStringAsync();
                if (HttpContext.Current.Request.ContentType == "application/json")
                {
                    using (var input = new StringReader(result))
                    {
                        try
                        {
                            JObject reqParams = JObject.Parse(input.ReadToEnd());

                            if (string.IsNullOrEmpty((string)HttpContext.Current.Items[Constants.CLIENT_TAG]) &&
                                reqParams["clientTag"] != null)
                            {
                                //For logging
                                HttpContext.Current.Items[Constants.CLIENT_TAG] = reqParams["clientTag"];
                            }

                            if (reqParams["ks"] != null)
                                parseKS(actionContext, reqParams["ks"].ToObject<string>());

                            if (KS.GetFromRequest().UserType == KalturaSessionType.ADMIN && reqParams["user_id"] != null)                            
                                KS.GetFromRequest().UserId = reqParams["user_id"].ToObject<string>();                            

                            //if (reqParams["partner_id"] != null)
                            //    HttpContext.Current.Items.Add(REQUEST_PARTNER_ID, reqParams["partner_id"].ToObject(typeof(string)));

                            //Running on the expected method parameters
                            ParameterInfo[] parameters = methodInfo.GetParameters();

                            List<Object> methodParams = new List<object>();
                            foreach (var p in parameters)
                            {
                                if (reqParams[p.Name] == null && p.IsOptional)
                                {
                                    methodParams.Add(Type.Missing);
                                    continue;
                                }
                                else if (reqParams[p.Name] == null)
                                {
                                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Missing parameter {0}", p.Name));
                                    return;
                                }

                                try
                                {
                                    //We deserialize the object based on the method parameter type
                                    methodParams.Add(reqParams[p.Name].ToObject(p.ParameterType));
                                }
                                catch (Exception ex)
                                {
                                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Invalid parameter format {0}", p.Name));
                                    return;
                                }
                            }

                            HttpContext.Current.Items.Add(REQUEST_METHOD_PARAMETERS, methodParams);
                        }
                        catch (JsonReaderException ex)
                        {
                            createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidJSONRequest, "Invalid JSON");
                            return;
                        }
                    }
                }
                else if (HttpContext.Current.Request.ContentType == "text/xml" ||
                    HttpContext.Current.Request.ContentType == "application/xml")
                {
                    //TODO
                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "XML is currently not supported");
                    return;
                }
                else
                {
                    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.BadRequest, "Content type is invalid");
                    return;
                }
            }
            else if (actionContext.Request.Method == HttpMethod.Get)
            {
                var tokens = actionContext.Request.GetQueryNameValuePairs().ToDictionary((keyItem) => keyItem.Key,
                    (valueItem) => valueItem.Value);

                //Running on the expected method parameters
                ParameterInfo[] parameters = methodInfo.GetParameters();

                List<Object> methodParams = new List<object>();
                foreach (ParameterInfo p in parameters)
                {
                    if (p.ParameterType.IsPrimitive || p.ParameterType == typeof(string))
                    {
                        if (tokens.ContainsKey(p.Name))
                        {
                            //TODO: throw exception
                            var obj = Convert.ChangeType(tokens[p.Name], p.ParameterType);
                            methodParams.Add(obj);
                        }
                        else if (p.IsOptional)
                        {
                            methodParams.Add(Type.Missing);
                        }
                        else
                            throw new Exception("TODO");
                    }
                    else if (p.ParameterType.IsEnum)
                    {
                        //TODO
                        if (p.IsOptional)
                        {
                            methodParams.Add(Type.Missing);
                        }
                    }
                    else if (p.ParameterType.IsArray)
                    {                        
                        if (p.IsOptional)
                        {
                            methodParams.Add(Type.Missing);
                        }
                    }
                    else if (p.ParameterType.IsClass)
                    {
                        if (p.IsOptional)
                        {
                            methodParams.Add(Type.Missing);
                        }
                    }
                    else
                    {
                        if (p.IsOptional)
                        {
                            methodParams.Add(Type.Missing);
                        }
                    }
                    //if (reqParams[p.Name] == null && p.IsOptional)
                    //{
                    //    methodParams.Add(Type.Missing);
                    //    continue;
                    //}
                    //else if (reqParams[p.Name] == null)
                    //{
                    //    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Missing parameter {0}", p.Name));
                    //    return;
                    //}

                    //try
                    //{
                    //    //We deserialize the object based on the method parameter type
                    //    methodParams.Add(reqParams[p.Name].ToObject(p.ParameterType));
                    //}
                    //catch (Exception ex)
                    //{
                    //    createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidActionParameters, string.Format("Invalid parameter format {0}", p.Name));
                    //    return;
                    //}
                }

                HttpContext.Current.Items.Add(REQUEST_METHOD_PARAMETERS, methodParams);
            }

            base.OnActionExecuting(actionContext);
        }

        private static void createErrorResponse(HttpActionContext actionContext, int errorCode, string msg)
        {
            //We cannot use the ApiException* concept in Filters, so we manually invoke exceptions here.
            actionContext.Response = actionContext.Request.CreateResponse(new ApiException.ExceptionPayload()
            {
                code = errorCode,
                error = new HttpError() { ExceptionMessage = msg }
            });
        }

        private static void parseKS(HttpActionContext actionContext, string ksVal)
        {
            StringBuilder sb = new StringBuilder(ksVal);
            sb = sb.Replace("-", "+");
            sb = sb.Replace("_", "/");

            int groupId = 0;
            byte[] encryptedData = null;
            string encryptedDataStr = null;
            string[] ksParts = null;

            try
            {
                encryptedData = System.Convert.FromBase64String(sb.ToString());
                encryptedDataStr = System.Text.Encoding.ASCII.GetString(encryptedData);
                ksParts = encryptedDataStr.Split('|');
            }
            catch (Exception ex)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
            }

            if (ksParts.Length < 3 || ksParts[0] != "v2" || !int.TryParse(ksParts[1], out groupId))
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "Invalid KS format");
                return;
            }

            // get group secret
            Group group = GroupsManager.GetGroup(groupId);
            string adminSecret = group.UserSecret;

            // build KS
            KS ks = KS.CreateKSFromEncoded(encryptedData, groupId, adminSecret);

            if (!ks.IsValid)
            {
                createErrorResponse(actionContext, (int)WebAPI.Managers.Models.StatusCode.InvalidKS, "KS Expired");
                return;
            }

            ks.SaveOnRequest();
        }
    }
}