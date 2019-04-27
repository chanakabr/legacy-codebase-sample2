using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;

using Microsoft.Extensions.DependencyInjection;

namespace FtpApiServer.Authentication
{
    public static class KalturaOttAuthenticationProviderExtensions
    {
        /// <summary>
        /// Enables authentication using OttUser.Login api for Kaltura
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IFtpServerBuilder UseKalturaOttAuthentication(this IFtpServerBuilder builder)
        {
            builder.Services.AddScoped<IMembershipProvider, KalturaOttAuthenticationProvider>();
            return builder;
        }
    }
}