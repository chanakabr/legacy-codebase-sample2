using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace TVinciShared
{
    public static class AssemblyUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static void RedirectAssembly(string fromAssemblyShotName, string replacmentAssemblyShortName)
        {
            log.Info($"Adding custom resolver redirect rule form:{fromAssemblyShotName}, to:{replacmentAssemblyShortName}");
            ResolveEventHandler handler = null;
            handler = (sender, args) =>
            {
                // Use latest strong name & version when trying to load SDK assemblies
                var requestedAssembly = new AssemblyName(args.Name);
                log.Debug($"RedirectAssembly >  requesting:{requestedAssembly}; replacment from:{fromAssemblyShotName}, to:{replacmentAssemblyShortName}");
                if (requestedAssembly.Name == fromAssemblyShotName)
                {
                    try
                    {
                        log.Debug($"Redirecting Assembly {fromAssemblyShotName} to: {replacmentAssemblyShortName}");
                        var replacmentAssembly = Assembly.Load(replacmentAssemblyShortName);
                        return replacmentAssembly;
                    }
                    catch (Exception e)
                    {
                        log.Error($"ERROR while trying to provide replacement Assembly {fromAssemblyShotName} to: {replacmentAssemblyShortName}", e);
                        return null;
                    }
                }

                log.Debug($"Framework faild to find {requestedAssembly}, trying to provide replacment from:{fromAssemblyShotName}, to:{replacmentAssemblyShortName}");

                return null;
            };
        }
    }
}
