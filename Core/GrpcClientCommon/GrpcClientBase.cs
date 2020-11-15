using Grpc.Core;
using Grpc.Core.Interceptors;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace GrpcClientCommon
{
    public class GrpcClientBase
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected readonly Channel _channel;
        private readonly string _certFilePath;

        public GrpcClientBase(string address, string certFilePath)
        {
            _logger.Debug($"initilizing GRPC client for address: [{address}], certFilePath:[{certFilePath}]");
            _certFilePath = certFilePath;

            var useSSL = string.IsNullOrWhiteSpace(certFilePath);
            var creds = useSSL ? GetSSLCredentials() : ChannelCredentials.Insecure;

            _channel = new Channel(address, creds);
            _channel.Intercept(new TracingInterceptor());
        }

        private SslCredentials GetSSLCredentials()
        {
            string cert = System.IO.File.ReadAllText(_certFilePath);
            var sslCreds = new SslCredentials(cert);
            return sslCreds;
        }
    }
}
