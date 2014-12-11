using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public class NPVRProviderFactory
    {
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

        private NPVRProvider GetGroupNPVRImpl(int groupID)
        {
            NPVRProvider provider = NPVRProvider.None;
            if (!groupsToNPVRImplMapping.ContainsKey(groupID))
            {
                lock (dictMutex)
                {
                    if (!groupsToNPVRImplMapping.ContainsKey(groupID))
                    {
                        int npvrProviderID = UtilsDal.Get_NPVRProviderID(groupID);
                        if (Enum.IsDefined(typeof(NPVRProvider), npvrProviderID))
                        {
                            provider = (NPVRProvider)npvrProviderID;
                        }
                        else
                        {
                            Logger.Logger.Log("Error", string.Format("Unknown NPVR Provider ID extracted from DB. G ID: {0} , NPVR ID: {1}", groupID, npvrProviderID), "NPVRProviderFactory");
                        }
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

        public bool IsGroupHaveNPVRImpl(int groupID)
        {
            return GetProvider(groupID) != null;
        }

        public INPVRProvider GetProvider(int groupID)
        {
            INPVRProvider res = null;
            NPVRProvider provider = GetGroupNPVRImpl(groupID);
            switch (provider)
            {
                case NPVRProvider.None:
                    break;
                case NPVRProvider.AlcatelLucent:
                    res = new AlcatelLucentNPVR(groupID);
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
