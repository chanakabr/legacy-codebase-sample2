using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    public static class NPVRProviderFactory
    {
        private static readonly string FACTORY_LOG_FILE = "NPVRProviderFactory";

        public static INPVRProvider GetProvider(int groupID)
        {
            INPVRProvider res = null;
            int npvrProviderID = UtilsDal.Get_NPVRProviderID(groupID);
            if (Enum.IsDefined(typeof(NPVRProvider), npvrProviderID))
            {
                NPVRProvider provider = (NPVRProvider)npvrProviderID;
                switch (provider)
                {
                    case NPVRProvider.None:
                        throw new NotImplementedException(String.Concat("GroupID: ", groupID, " has no NPVR Provider configured."));
                    case NPVRProvider.AlcatelLucent:
                        res = new AlcatelLucentNPVR(groupID);
                        break;
                    case NPVRProvider.Kaltura:
                    case NPVRProvider.Harmonic:
                    default:
                        throw new NotImplementedException(String.Concat("NPVR Provider not implemented. ID: ", npvrProviderID, " Name: ", provider.ToString()));
                }
            }
            else
            {
                string msg = String.Concat("No NPVRProvider enum corresponds to: ", npvrProviderID);
                Logger.Logger.Log("Error", msg, FACTORY_LOG_FILE);
                throw new NotImplementedException(msg);
            }

            return res;
        }
    }
}
