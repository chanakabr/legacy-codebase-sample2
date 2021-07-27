using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using ConfigurationManager;
using ConfigurationManager.Types;
using Elasticsearch.Net;
using KLogMonitor;
using Nest;
using Level = log4net.Core.Level;

namespace ElasticSearch.NEST
{
    public class NESTFactory
    {
        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static object _padlock = new object();
        private static IElasticClient _instance;


        public static IElasticClient GetInstance(IApplicationConfiguration appConfig)
        {
            if (_instance == null)
            {
                lock (_padlock)
                {
                    if (_instance == null)
                    {
                        _instance = GetNewNESTInstance(appConfig);
                    }
                }
            }

            return _instance;
        }


        private static IElasticClient GetNewNESTInstance(IApplicationConfiguration appConfig)
        {
            _log.Info($"constructing new instance of NEST, url:[{appConfig.ElasticSearchConfiguration.URL_V7_13.Value}]");
            var uri = new Uri(appConfig.ElasticSearchConfiguration.URL_V7_13.Value);
            var pool = new SingleNodeConnectionPool(uri);

            var settings = new ConnectionSettings(pool)
                // This can be used wither by Remote Task or any other phoenix component so we take the entry assembly name as the user agent
                // this will allow us to see who actually called ES form es logs
                .UserAgent(Assembly.GetEntryAssembly()?.GetName().Name)
                // required for response to be logged
                .DisableDirectStreaming()
                .ConnectionLimit(appConfig.ElasticSearchHttpClientConfiguration.MaxConnectionsPerServer.Value)
                .OnRequestDataCreated(HandleLoggingCreatedRequest)
                .OnRequestCompleted(HandleElasticsearchRequestLogging);
            var esClient = new ElasticClient(settings);
            return esClient;
        }

        private static void HandleLoggingCreatedRequest(RequestData requestData)
        {
            try
            {
                // if no post data then just log the path an query
                if (requestData.PostData == null)
                {
                    _log.Debug($"es request starting: {requestData.Method} {requestData.PathAndQuery}");
                }
                else
                {
                    LogPostData(requestData);
                }
            }
            catch (Exception e)
            {
                _log.Warn($"failed to construct request log to Elasticsearch", e);
            }
        }

        private static void LogPostData(RequestData requestData)
        {
            // do all buffering and stream work only if we really enabled debug log
            if (KLogger.GetLogLevel() <= Level.Debug)
            {
                using (var ms = new MemoryStream())
                using (var sr = new StreamReader(ms))
                {
                    requestData.PostData.Write(ms, new ConnectionSettings());
                    ms.Flush();
                    ms.Seek(0, SeekOrigin.Begin);
                    var data = sr.ReadToEnd();
                    _log.Debug($"es request starting: {requestData.Method} {requestData.PathAndQuery} [{data}]");
                }
            }
        }

        private static void HandleElasticsearchRequestLogging(IApiCallDetails apiCallDetails)
        {
            try
            {
                var sb = new StringBuilder();
                // log out the request and the request body, if one exists for the type of request
                if (apiCallDetails.RequestBodyInBytes != null)
                {
                    sb.Append($"es request sent: {apiCallDetails.HttpMethod} {apiCallDetails.Uri} {Encoding.UTF8.GetString(apiCallDetails.RequestBodyInBytes)}");
                }
                else
                {
                    sb.Append($"es request sent: {apiCallDetails.HttpMethod} {apiCallDetails.Uri}");
                }

                // log out the response and the response body, if one exists for the type of response
                if (apiCallDetails.ResponseBodyInBytes != null)
                {
                    sb.Append($" response: {apiCallDetails.HttpStatusCode} {Encoding.UTF8.GetString(apiCallDetails.ResponseBodyInBytes)}");
                }
                else
                {
                    sb.Append($" response: {apiCallDetails.HttpStatusCode}");
                }

                _log.Debug(sb.ToString());
            }
            catch (Exception e)
            {
                _log.Warn($"failed to construct response log to Elasticsearch", e);
            }
        }
    }
}