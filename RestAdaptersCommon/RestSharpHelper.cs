using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using KLogMonitor;
using RestSharp;

namespace RestAdaptersCommon
{
    public static class RestSharpHelper
    {
        private static int _RequestCounter = 0;
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static IRestResponse MakeRequest(this IRestClient client, IRestRequest request)
        {
            return Task.Run(() => MakeRequestAsync(client, request)).ConfigureAwait(false).GetAwaiter().GetResult();
        }


        public static async Task<IRestResponse> MakeRequestAsync(this IRestClient client, IRestRequest request)
        {
            IRestResponse response = null;
            var stopWatch = new Stopwatch();
            try
            {
                request.AddHeader(KLogMonitor.Constants.REQUEST_ID_KEY, KLogger.GetRequestId());
                response = await client.ExecuteTaskAsync(request);
                if (response.ResponseStatus != ResponseStatus.Completed)
                {
                    throw new Exception($"Could not complete request, reason:[{response.ResponseStatus}], message:[{response.ErrorMessage}] excpetion:[{response.ErrorException}]");
                }
            }
            catch (Exception e)
            {
                _Logger.Error($"Error while making request:[{request.Resource}]", e);
                throw;
            }
            finally
            {
                var elapsedMs = stopWatch.ElapsedMilliseconds;
                LogRequest(client, request, response, elapsedMs);
            }
            return response;
        }

        private static void LogRequest(IRestClient client, IRestRequest request, IRestResponse response, long durationMs)
        {
            var requestID = Interlocked.Increment(ref _RequestCounter);

            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),

                // This will generate the actual Uri used in the request
                uri = client.BuildUri(request),
            };

            var body = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.RequestBody);
            var headers = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.HttpHeader);
            var cookie = request.Parameters.FirstOrDefault(p => p.Type == ParameterType.Cookie);

            _Logger.Debug($"Request ({requestID:X4}): [{requestToLog.method}] [{requestToLog.uri}]");
            _Logger.Debug($"Request Body ({requestID:X4}): [{body}]");
            _Logger.Debug($"Request Headers ({requestID:X4}): [{headers}]");
            _Logger.Debug($"Request cookie ({requestID:X4}): [{cookie}]");


            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            _Logger.Debug($"Response ({requestID:X4}): ({responseToLog.statusCode}) [{response.Content}]");
            _Logger.Debug($"Response Headers ({requestID:X4}): [{string.Join(",", response.Headers)}]");

        }
    }
}
