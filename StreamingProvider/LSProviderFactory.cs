using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace StreamingProvider
{
    public static class LSProviderFactory
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static object tokenImplLocker = new object();
        private static Dictionary<string, ILSProvider> dLSProviderImpl = new Dictionary<string, ILSProvider>();

        public static ILSProvider GetLSProvidernstance(string CdnStrID)
        {
            ILSProvider provider = null;
            if (CdnStrID != null)
            {
                string cdn = CdnStrID.ToUpper();
                if (!dLSProviderImpl.ContainsKey(cdn))
                {
                    lock (tokenImplLocker)
                    {
                        if (!dLSProviderImpl.ContainsKey(cdn))
                        {
                            ILSProvider tempProvider = CreateProvider(cdn);

                            if (tempProvider != null)
                            {
                                dLSProviderImpl[cdn] = tempProvider;
                            }
                        }
                    }
                }

                dLSProviderImpl.TryGetValue(cdn, out provider);
            }
            return provider;
        }

        private static ILSProvider CreateProvider(string CdnStrID)
        {
            BaseLSProvider provider;

            try
            {
                switch (CdnStrID)
                {
                    case "ALCATELLUCENT":
                        provider = new AlcatellLucentProvider();
                        break;
                    case "HARMONIC":
                        provider = new HarmonicProvider();
                        break;
                    default:
                        provider = new BaseLSProvider();
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                provider = null;
            }

            return provider;
        }
    }
}
