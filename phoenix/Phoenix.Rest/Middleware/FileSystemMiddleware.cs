using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ConfigurationManager;
using KLogMonitor;
using System.Reflection;

namespace Phoenix.Rest.Middleware
{
    public static class FileSystemMiddleware
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Configures local file system according to TCM configuration under section FileUpload.
        /// See UploadToken..Add, UploadToken..Upload.
        /// </summary>
        /// <param name="app"></param>
        public static void UsePhoenixLocalFileSystem(this IApplicationBuilder app)
        {
            if (ApplicationConfiguration.Current.FileUpload.Type.Value != ConfigurationManager.Types.eFileUploadType.FileSystem)
                return;

            string destinationPath = ApplicationConfiguration.Current.FileUpload.FileSystem.DestPath.Value;
            string requestPath = ApplicationConfiguration.Current.FileUpload.FileSystem.PublicUrl.Value;

            log.Debug($"Setting local file system to {destinationPath} on {requestPath}");

            try
            {
                if (!string.IsNullOrEmpty(destinationPath) && !string.IsNullOrEmpty(requestPath))
                {
                    if (!Directory.Exists(destinationPath))
                    {
                        Directory.CreateDirectory(destinationPath);
                    }

                    if (Uri.TryCreate(requestPath, UriKind.RelativeOrAbsolute, out var uri))
                    {
                        app.UseStaticFiles(new StaticFileOptions
                        {
                            FileProvider = new PhysicalFileProvider(destinationPath),
                            RequestPath = uri.LocalPath.TrimEnd('/')
                        });

                        log.Debug("Local file system successfully configured");
                    }
                }
            }
            catch (Exception e)
            {
                log.Error($"Error while configuring local file system to {destinationPath} on {requestPath}", e);
            }
        }
    }
}