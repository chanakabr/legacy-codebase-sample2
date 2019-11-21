using ConfigurationManager;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace TVinciShared
{
    public static class HttpClientUtil
    {

        static readonly List<SslProtocols> enabledSslProtocols = ApplicationConfiguration.HttpClientConfiguration.GetSslProtocols();
        static readonly List<DecompressionMethods> enabledDecompressionMethod = ApplicationConfiguration.HttpClientConfiguration.GetDecompressionMethods();
        static readonly int maxConnectionsPerServer = ApplicationConfiguration.HttpClientConfiguration.MaxConnectionsPerServer.IntValue;
        static readonly bool checkCertificateRevocationList = ApplicationConfiguration.HttpClientConfiguration.CheckCertificateRevocationList.Value;
        static readonly System.TimeSpan timeout = System.TimeSpan.FromMilliseconds(ApplicationConfiguration.HttpClientConfiguration.TimeOutInMiliSeconds.DoubleValue);

        public static HttpClient GetHttpClient()
        {
#if NETCOREAPP3_0
            SocketsHttpHandler httpHandler = new SocketsHttpHandler() { SslOptions = new System.Net.Security.SslClientAuthenticationOptions() };
            foreach (SslProtocols sslProtocols in enabledSslProtocols)
            {
                httpHandler.SslOptions.EnabledSslProtocols = sslProtocols | httpHandler.SslOptions.EnabledSslProtocols;
            }

            foreach (DecompressionMethods decompressionMethod in enabledDecompressionMethod)
            {
                httpHandler.AutomaticDecompression = decompressionMethod | httpHandler.AutomaticDecompression;
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

            foreach (DecompressionMethods decompressionMethod in enabledDecompressionMethod)
            {
                httpHandler.AutomaticDecompression = decompressionMethod | httpHandler.AutomaticDecompression;
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

    }
}
