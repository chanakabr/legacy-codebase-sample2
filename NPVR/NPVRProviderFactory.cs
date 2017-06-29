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

        private Dictionary<int, NPVRProviderImp> groupsToNPVRImplMapping = null;

        private NPVRProviderFactory()
        {
            groupsToNPVRImplMapping = new Dictionary<int, NPVRProviderImp>();
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
            NPVRProviderImp providerImp = new NPVRProviderImp(NPVRProvider.None, false);
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
                            providerImp.npvrProvider = (NPVRProvider)npvrProviderID;
                        }
                        else
                        {
                            log.Error("Error - " + string.Format("Unknown NPVR Provider ID extracted from DB. G ID: {0} , NPVR ID: {1}", groupID, npvrProviderID));
                        }

                        groupsToNPVRImplMapping.Add(groupID, new NPVRProviderImp(providerImp.npvrProvider, synchronizeNpvrWithDomain));
                    }
                    else
                    {
                        providerImp = groupsToNPVRImplMapping[groupID];
                        synchronizeNpvrWithDomain = providerImp.synchronizeNpvrWithDomain;
                    }
                }
            }
            else
            {
                providerImp = groupsToNPVRImplMapping[groupID];
                synchronizeNpvrWithDomain = providerImp.synchronizeNpvrWithDomain;
            }

            return providerImp.npvrProvider;
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
