using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Core.Middleware
{
    public static class UrlMappingHelper
    {
        public static IApplicationBuilder MapEndpoint(this IApplicationBuilder app, string url, Action<IApplicationBuilder> configuration)
        {
            app.MapWhen(context =>
            {
                var incomingUrlSegments = context.Request.Path.ToString().Split('/', StringSplitOptions.RemoveEmptyEntries);
                var matchUrlSegments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);

                // todo: sequance equal is not right here .. sometimes its just the begging of the path
                return incomingUrlSegments.Length >= matchUrlSegments.Length 
                    && incomingUrlSegments.SequenceEqual(matchUrlSegments, StringComparer.OrdinalIgnoreCase);

            }, endpointApplication =>
            {
                configuration(endpointApplication);
            });
            return app;
        }
    }
}
