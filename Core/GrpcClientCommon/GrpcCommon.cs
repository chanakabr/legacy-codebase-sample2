using System.IO;
using Grpc.Core;
using Grpc.Core.Interceptors;
using KLogMonitor;
using System.Reflection;

namespace GrpcClientCommon
{
    public static class GrpcCommon
    {
        private static readonly KLogger Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        public static CallInvoker CreateChannel(string address, string certFilePath)
        {
            Logger.Debug($"initializing GRPC client for address: [{address}], certFilePath:[{certFilePath}]");
            var credentials = GetSslCredentials(certFilePath);

            var channel = new Channel(address, credentials)
                .Intercept(new TracingInterceptor());
            return channel;
        }

        public static ChannelCredentials GetSslCredentials(string certFilePath)
        {
            var useSsl = !string.IsNullOrWhiteSpace(certFilePath);
            if (!useSsl) return ChannelCredentials.Insecure;
            
            var cert = File.ReadAllText(certFilePath);
            var sslCredentials = new SslCredentials(cert);
            return sslCredentials;
        }
    }
}
