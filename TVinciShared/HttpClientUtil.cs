using ConfigurationManager;
using ConfigurationManager.Types;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TVinciShared
{
    public static class HttpClientUtil
    {
        static readonly List<SslProtocols> defaultEnabledSslProtocols = ApplicationConfiguration.HttpClientConfiguration.GetSslProtocols();
        static readonly List<DecompressionMethods> defaultEnabledDecompressionMethod = ApplicationConfiguration.HttpClientConfiguration.GetDecompressionMethods();
        static readonly int defaultMaxConnectionsPerServer = ApplicationConfiguration.HttpClientConfiguration.MaxConnectionsPerServer.IntValue;
        static readonly bool defaultCheckCertificateRevocationList = ApplicationConfiguration.HttpClientConfiguration.CheckCertificateRevocationList.Value;
        static readonly System.TimeSpan defaultTimeout = System.TimeSpan.FromMilliseconds(ApplicationConfiguration.HttpClientConfiguration.TimeOutInMiliSeconds.DoubleValue);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specificConfiguration"></param>
        /// <param name="shouldDecompress">Should the http client be configured with decompression method from TCM or not.</param>
        /// <returns></returns>
        public static HttpClient GetHttpClient(HttpClientConfiguration specificConfiguration = null, bool shouldDecompress = true)
        {
            List<SslProtocols> enabledSslProtocols;
            List<DecompressionMethods> enabledDecompressionMethod;
            int maxConnectionsPerServer;
            bool checkCertificateRevocationList;
            System.TimeSpan timeout;

            GetConfigurationValues(specificConfiguration, 
                out enabledSslProtocols, out enabledDecompressionMethod, out maxConnectionsPerServer, out checkCertificateRevocationList, out timeout);

#if NETCOREAPP3_1
            SocketsHttpHandler httpHandler = new SocketsHttpHandler() { SslOptions = new System.Net.Security.SslClientAuthenticationOptions() };
            foreach (SslProtocols sslProtocols in enabledSslProtocols)
            {
                httpHandler.SslOptions.EnabledSslProtocols = sslProtocols | httpHandler.SslOptions.EnabledSslProtocols;
            }

            if (shouldDecompress)
            {
                foreach (DecompressionMethods decompressionMethod in enabledDecompressionMethod)
                {
                    httpHandler.AutomaticDecompression = decompressionMethod | httpHandler.AutomaticDecompression;
                }
            }

            httpHandler.MaxConnectionsPerServer = maxConnectionsPerServer;
            httpHandler.SslOptions.CertificateRevocationCheckMode = checkCertificateRevocationList ? X509RevocationMode.Online : X509RevocationMode.NoCheck;
            if (!checkCertificateRevocationList)
            {
                httpHandler.SslOptions.RemoteCertificateValidationCallback = delegate { return true; };
            }

            HttpClient httpClient = new HttpClient(httpHandler) { Timeout = timeout };
            return httpClient;
#elif NETFRAMEWORK
            HttpClientHandler httpHandler = new HttpClientHandler() { SslProtocols = new SslProtocols() };
            foreach (SslProtocols sslProtocols in enabledSslProtocols)
            {
                httpHandler.SslProtocols = sslProtocols | httpHandler.SslProtocols;
            }

            if (shouldDecompress)
            {
                foreach (DecompressionMethods decompressionMethod in enabledDecompressionMethod)
                {
                    httpHandler.AutomaticDecompression = decompressionMethod | httpHandler.AutomaticDecompression;
                }
            }

            httpHandler.MaxConnectionsPerServer = maxConnectionsPerServer;
            httpHandler.CheckCertificateRevocationList = checkCertificateRevocationList;
            if (!checkCertificateRevocationList)
            {
                httpHandler.ServerCertificateCustomValidationCallback = delegate { return true; };
            }

            HttpClient httpClient = new HttpClient(httpHandler) { Timeout = timeout };
            return httpClient;
#endif
        }

        private static void GetConfigurationValues(HttpClientConfiguration specificConfiguration, out List<SslProtocols> enabledSslProtocols, out List<DecompressionMethods> enabledDecompressionMethod, out int maxConnectionsPerServer, out bool checkCertificateRevocationList, out System.TimeSpan timeout)
        {
            bool shouldTakeSpecificConfiguration = specificConfiguration != null && !specificConfiguration.IsEmpty;

            // enabled ssl protocols
            enabledSslProtocols = shouldTakeSpecificConfiguration ?
                specificConfiguration.GetSslProtocols() : defaultEnabledSslProtocols;

            // enabled decompression method
            enabledDecompressionMethod = shouldTakeSpecificConfiguration ?
                specificConfiguration.GetDecompressionMethods() : defaultEnabledDecompressionMethod;

            // max connections per server
            maxConnectionsPerServer = shouldTakeSpecificConfiguration ?
                specificConfiguration.MaxConnectionsPerServer.IntValue : defaultMaxConnectionsPerServer;

            // check certificate revocation list
            checkCertificateRevocationList = shouldTakeSpecificConfiguration ?
                specificConfiguration.CheckCertificateRevocationList.Value : defaultCheckCertificateRevocationList;

            // timeout
            timeout = shouldTakeSpecificConfiguration ?
                System.TimeSpan.FromMilliseconds(specificConfiguration.TimeOutInMiliSeconds.DoubleValue) : defaultTimeout;
        }
    }
}
