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
using KlogMonitorHelper;
using Newtonsoft.Json;
using ConfigurationManager;

namespace WebAPI.Filters
{
    public class WrappingHandler : DelegatingHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string MUTLIREQUEST_ACTION = "multirequest";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {

            // get request ID
            HttpContext.Current.Items[Constants.REQUEST_ID_KEY] = request.GetCorrelationId();
            var requestIdFromRequestHeader = HttpContext.Current.Request.Headers[Constants.REQUEST_ID_KEY];
            if (!string.IsNullOrEmpty(requestIdFromRequestHeader))
            {
                HttpContext.Current.Items[Constants.REQUEST_ID_KEY] = requestIdFromRequestHeader;
            }

            byte[] requestBody = await request.Content.ReadAsByteArrayAsync();
            HttpContext.Current.Items["body"] = requestBody;

            var loggingContext = new ContextData();
            loggingContext.Load();

            // log request body
            string encodedBody = Encoding.UTF8.GetString(requestBody);
            bool shouldLogRawRequest = !(encodedBody.IndexOf("password", StringComparison.OrdinalIgnoreCase) >= 0) && !(encodedBody.IndexOf("mail", StringComparison.OrdinalIgnoreCase) >= 0);

            if (shouldLogRawRequest)
            {
                log.Debug($"API Request - {request.RequestUri.OriginalString} {encodedBody}");
            }

            ExtractActionToLog(request.RequestUri);
            bool isMultirequest = HttpContext.Current.Items[Constants.ACTION] != null
                                  && HttpContext.Current.Items[Constants.ACTION].ToString() == MUTLIREQUEST_ACTION;

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_CLIENT_API_START))
            {
                //let other handlers process the request
                var response = await base.SendAsync(request, cancellationToken);
                var wrappedResponse = await BuildApiResponse(request, response, float.Parse(km.ExecutionTime));
                if (isMultirequest)
                {
                    HttpContext.Current.Items[Constants.ACTION] = MUTLIREQUEST_ACTION;
                    HttpContext.Current.Items[Constants.MULTIREQUEST] = "0";
                }

                return wrappedResponse;
            }
        }

        private async static Task<HttpResponseMessage> BuildApiResponse(HttpRequestMessage request, HttpResponseMessage response, float executionTime)
        {
            object content = null;
            string message = "";
            int subCode = (int)StatusCode.OK;
            response.TryGetContentValue(out content);

            if (content is ApiException.ExceptionPayload && ((ApiException.ExceptionPayload)content).code != 0)
            {
                WebAPI.Exceptions.ApiException.ExceptionPayload payload = content as WebAPI.Exceptions.ApiException.ExceptionPayload;
                subCode = payload.code;
                message = KalturaApiExceptionHelpers.HandleError(payload.error.ExceptionMessage, payload.error.StackTrace);
                content = KalturaApiExceptionHelpers.prepareExceptionResponse(payload.code, message, payload.arguments);
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
                content = KalturaApiExceptionHelpers.prepareExceptionResponse((int)StatusCode.BadRequest, "Bad request");
                response.StatusCode = System.Net.HttpStatusCode.OK;
                message = KalturaApiExceptionHelpers.HandleError("Bad Request", "");
            }
            else
            {
                content = KalturaApiExceptionHelpers.prepareExceptionResponse((int)StatusCode.Error, "Unknown error");
                subCode = (int)StatusCode.Error;
                message = KalturaApiExceptionHelpers.HandleError("Unknown error", "");
            }

            if (!response.Headers.Contains("X-Kaltura"))
            {
                //We never return 500. even on errors/warning. update - only if specifically changed
                response.StatusCode = System.Net.HttpStatusCode.OK;
            }

            Guid reqID = request.GetCorrelationId();
            var newResponse = request.CreateResponse(response.StatusCode, new StatusWrapper(subCode, reqID, executionTime, content, message));

            newResponse.Headers.Add("X-Kaltura-Session", HttpContext.Current.Items[Constants.REQUEST_ID_KEY].ToString());
            newResponse.Headers.Add("Access-Control-Allow-Origin", "*");

            foreach (var header in response.Headers)
            {
                newResponse.Headers.Add(header.Key, header.Value);
            }

            // BEO-7013 - add all invalidation keys to header
            if (ApplicationConfiguration.Current.ShouldAddInvalidationKeysToHeader.Value)
            {
                if (HttpContext.Current?.Items != null && HttpContext.Current.Items[CachingProvider.LayeredCache.LayeredCache.CURRENT_REQUEST_LAYERED_CACHE] != null &&
                    HttpContext.Current.Items[CachingProvider.LayeredCache.LayeredCache.CURRENT_REQUEST_LAYERED_CACHE] is CachingProvider.LayeredCache.RequestLayeredCache)
                {
                    var requestLayeredCache = HttpContext.Current.Items[CachingProvider.LayeredCache.LayeredCache.CURRENT_REQUEST_LAYERED_CACHE] as
                        CachingProvider.LayeredCache.RequestLayeredCache;
                    string invalidationKeysString = string.Join(";", requestLayeredCache.invalidationKeysToKeys.Keys);
                    newResponse.Headers.Add(CachingProvider.LayeredCache.LayeredCache.INVALIDATION_KEYS_HEADER, invalidationKeysString);
                }
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
                                bool isReadAction = CachingProvider.LayeredCache.LayeredCache.readActions.Contains(action);
                                HttpContext.Current.Items[CachingProvider.LayeredCache.LayeredCache.IS_READ_ACTION] = isReadAction;

                                // add action to log
                                HttpContext.Current.Items[Constants.ACTION] = string.Format("{0}.{1}",
                                    string.IsNullOrEmpty(service) ? "null" : service,
                                    string.IsNullOrEmpty(action) ? "null" : action);
                                isActionExtracted = true;
                            }
                            else
                            {
                                HttpContext.Current.Items[Constants.ACTION] = MUTLIREQUEST_ACTION;
                                isActionExtracted = true;
                            }

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
    }
}
