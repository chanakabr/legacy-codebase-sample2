using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;

namespace NPVR
{
    public class NPVRProviderFactory
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly object mutex = new object();
        private static readonly object dictMutex = new object();
        private static NPVRProviderFactory instance = null;

        private Dictionary<int, NPVRProvider> groupsToNPVRImplMapping = null;

        private NPVRProviderFactory()
        {
            groupsToNPVRImplMapping = new Dictionary<int, NPVRProvider>();
        }

        public static NPVRProviderFactory Instance()
        {
            if (instance == null)
            {
                lock (mutex)
                {
                    if (instance == null)
                    {
                        instance = new NPVRProviderFactory();
                    }
                }
            }

            return instance;
        }

        private NPVRProvider GetGroupNPVRImpl(int groupID, out bool synchronizeNpvrWithDomain)
        {
            NPVRProvider provider = NPVRProvider.None;
            synchronizeNpvrWithDomain = false;

            if (!groupsToNPVRImplMapping.ContainsKey(groupID))
            {
                lock (dictMutex)
                {
                    if (!groupsToNPVRImplMapping.ContainsKey(groupID))
                    {
                        int npvrProviderID = UtilsDal.Get_NPVRProviderID(groupID, out synchronizeNpvrWithDomain);
                        if (Enum.IsDefined(typeof(NPVRProvider), npvrProviderID))
                        {
                            provider = (NPVRProvider)npvrProviderID;
                        }
                        else
                        {
                            log.Error("Error - " + string.Format("Unknown NPVR Provider ID extracted from DB. G ID: {0} , NPVR ID: {1}", groupID, npvrProviderID));
                        }

                        groupsToNPVRImplMapping.Add(groupID, provider);
                    }
                    else
                    {
                        provider = groupsToNPVRImplMapping[groupID];
                    }
                }
            }
            else
            {
                provider = groupsToNPVRImplMapping[groupID];
            }

            return provider;
        }

        public bool IsGroupHaveNPVRImpl(int groupID, out INPVRProvider npvr)
        {
            npvr = GetProvider(groupID);
            return npvr != null;
        }

        public INPVRProvider GetProvider(int groupID)
        {
            INPVRProvider res = null;
            bool synchronizeNpvrWithDomain = false;
            NPVRProvider provider = GetGroupNPVRImpl(groupID, out synchronizeNpvrWithDomain);
            switch (provider)
            {
                case NPVRProvider.None:
                    break;
                case NPVRProvider.AlcatelLucent:
                    res = new AlcatelLucentNPVR(groupID, synchronizeNpvrWithDomain);
                    break;
                case NPVRProvider.Kaltura:
                // fall through
                case NPVRProvider.Harmonic:
                // fall through
                default:
                    break;
            }

            return res;
        }
    }
}
