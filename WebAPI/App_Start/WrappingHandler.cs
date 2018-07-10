using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using KLogMonitor;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace WebAPI.App_Start
{
    public partial class KalturaApiExceptionArg : KalturaOTTObject
    {
        /// <summary>
        /// Argument name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        /// <summary>
        /// Argument value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        public string value { get; set; }
    }


    [DataContract(Name = "error")]
    public partial class KalturaAPIExceptionWrapper : KalturaSerializable
    {
        [DataMember(Name = "error")]
        [JsonProperty(PropertyName = "error")]
        public KalturaAPIException error { get; set; }
    }

    public partial class KalturaAPIException : KalturaSerializable
    {
        [JsonProperty(PropertyName = "objectType")]
        [DataMember(Name = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }

        [JsonProperty(PropertyName = "code")]
        [DataMember(Name = "code")]
        public string code { get; set; }

        [JsonProperty(PropertyName = "message")]
        [DataMember(Name = "message")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "args")]
        [DataMember(Name = "args")]
        public List<KalturaApiExceptionArg> args { get; set; }
    }


    public class WrappingHandler : DelegatingHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MUTLIREQUEST = "multirequest";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            // get request ID
            HttpContext.Current.Items[Constants.REQUEST_ID_KEY] = request.GetCorrelationId();

            // log request body
            log.DebugFormat("API Request - {0} {1}",
                            request.RequestUri.OriginalString,            // 0
                            await request.Content.ReadAsStringAsync());   // 1 

            ExtractActionToLog(request.RequestUri);

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
            {
                //let other handlers process the request
                var response = await base.SendAsync(request, cancellationToken);
                var wrappedResponse = await BuildApiResponse(request, response, float.Parse(km.ExecutionTime));
                return wrappedResponse;
            }
        }

        private async static Task<HttpResponseMessage> BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response, float executionTime)
        {
            if (request.GetRouteData().Route.RouteTemplate.ToLower().Contains("swagger"))
                return response;

            object content = null;
            string message = "";
            int subCode = (int)StatusCode.OK;
            response.TryGetContentValue(out content);

            if (content is ApiException.ExceptionPayload && ((ApiException.ExceptionPayload)content).code != 0)
            {
                WebAPI.Exceptions.ApiException.ExceptionPayload payload = content as WebAPI.Exceptions.ApiException.ExceptionPayload;
                subCode = payload.code;
                message = HandleError(payload.error.ExceptionMessage, payload.error.StackTrace);
                content = prepareExceptionResponse(payload.code, message, payload.arguments);
                if (payload.failureHttpCode != System.Net.HttpStatusCode.OK && payload.failureHttpCode != 0)
                {
                    response.StatusCode = payload.failureHttpCode;
                    response.Headers.Add("X-Kaltura-App", string.Format("exiting on error {0} - {1}", payload.code, message));
                    response.Headers.Add("X-Kaltura", string.Format("error-{0}", payload.code));
                }
            }
            else if (response.IsSuccessStatusCode)
            {
                message = "success";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                //Web API Bad Request global error
                content = prepareExceptionResponse((int)StatusCode.BadRequest, "Bad request");
                response.StatusCode = System.Net.HttpStatusCode.OK;
                message = HandleError("Bad Request", "");
            }
            else
            {
                content = prepareExceptionResponse((int)StatusCode.Error, "Unknown error");
                subCode = (int)StatusCode.Error;
                message = HandleError("Unknown error", "");
            }

            if (!response.Headers.Contains("X-Kaltura"))
            {
                //We never return 500. even on errors/warning. update - only if specifically changed
                response.StatusCode = System.Net.HttpStatusCode.OK;
            }
            Guid reqID = request.GetCorrelationId();
            var newResponse = request.CreateResponse(response.StatusCode, new StatusWrapper(subCode, reqID, executionTime, content, message));

            newResponse.Headers.Add("X-Kaltura-Session", reqID.ToString());
            newResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            return newResponse;
        }

        private static void ExtractActionToLog(Uri uri)
        {
            var segments = uri.Segments;
            bool isActionExtracted = false;
            try
            {
                if (segments != null && segments.Length > 0)
                {
                    for (int i = 0; i < segments.Length; i++)
                    {
                        if (segments[i].ToLower() == "service/")
                        {
                            string service = string.Empty;
                            string action = string.Empty;
                            if (i + 3 < segments.Length)
                            {
                                service = segments[i + 1].Replace("/", string.Empty);
                                action = segments[i + 3].Replace("/", string.Empty);
                            }
                            else
                            {
                                service = MUTLIREQUEST;
                                action = MUTLIREQUEST;
                            }

                            // add action to log
                            HttpContext.Current.Items[Constants.ACTION] = string.Format("{0}.{1}",
                                string.IsNullOrEmpty(service) ? "null" : service,
                                string.IsNullOrEmpty(action) ? "null" : action);
                            isActionExtracted = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while trying to extract action+service from request URL. Original request: {0}, ex: {1}", uri.OriginalString, ex);
            }

            if (!isActionExtracted)
                log.WarnFormat("Could not extract action + service from request query. Original request: {0}", uri.OriginalString);
        }

        public static KalturaAPIExceptionWrapper prepareExceptionResponse(int statusCode, string msg, KalturaApiExceptionArg[] arguments = null)
        {
            return new KalturaAPIExceptionWrapper() { error = new KalturaAPIException() { message = msg, code = statusCode.ToString(), args = arguments == null ? null : arguments.ToList() } };
        }

        public static string HandleError(string errorMsg, string stack)
        {
            string message = errorMsg;
#if DEBUG
            message = string.Concat(message, stack);
            log.ErrorFormat("{0}", message);
#else
            log.ErrorFormat("{0} {1}", message, stack);
#endif


            return message;
        }
    }
}
