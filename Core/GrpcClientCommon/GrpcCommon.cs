using System;
using System.IO;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Phx.Lib.Log;
using System.Reflection;
using Phx.Lib.Appconfig.Types;

namespace GrpcClientCommon
{
    public static class GrpcCommon
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static CallInvoker CreateChannel(BaseGrpcMicroserviceConfiguration configuration) =>
            CreateChannel(configuration.Address.Value, configuration.CertFilePath.Value,
                configuration.RetryCount.Value);
        
        public static CallInvoker CreateChannel(string address, string certFilePath, int retryCount)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentException("GRPC address is empty", nameof(address));
            
            Logger.Debug($"initializing GRPC client for address: [{address}], certFilePath:[{certFilePath}]");
            var credentials = GetSslCredentials(certFilePath);

            var channel = new Channel(address, credentials)
                .Intercept(new GrpcRequestInterceptor(address, retryCount));
            return channel;
        }

        private static ChannelCredentials GetSslCredentials(string certFilePath)
        {
            var useSsl = !string.IsNullOrWhiteSpace(certFilePath);
            if (!useSsl) return ChannelCredentials.Insecure;
            
            var cert = File.ReadAllText(certFilePath);
            var sslCredentials = new SslCredentials(cert);
            return sslCredentials;
        }
    }
}
