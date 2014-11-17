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
        //private static readonly string FACTORY_LOG_FILE = "NPVRProviderFactory";

        public static bool IsGroupHaveNPVRImpl(int groupID)
        {
            NPVRProvider provider = NPVRProvider.None;

            return IsGroupHaveNPVRImpl(groupID, ref provider);
        }

        private static bool IsGroupHaveNPVRImpl(int groupID, ref NPVRProvider provider)
        {
            bool res = false;
            int npvrProviderID = UtilsDal.Get_NPVRProviderID(groupID);
            if (Enum.IsDefined(typeof(NPVRProvider), npvrProviderID))
            {
                provider = (NPVRProvider)npvrProviderID;
                switch (provider)
                {
                    case NPVRProvider.AlcatelLucent:
                        res = true;
                        break;
                    case NPVRProvider.Kaltura:
                    case NPVRProvider.Harmonic:
                    default:
                        break;
                }
            }
            else
            {
                provider = NPVRProvider.None;
            }

            return res;
        }

        public static INPVRProvider GetProvider(int groupID)
        {
            INPVRProvider res = null;
            //int npvrProviderID = UtilsDal.Get_NPVRProviderID(groupID);
            //if (Enum.IsDefined(typeof(NPVRProvider), npvrProviderID))
            //{
            //    NPVRProvider provider = (NPVRProvider)npvrProviderID;
            //    switch (provider)
            //    {
            //        case NPVRProvider.None:
            //            throw new NotImplementedException(String.Concat("GroupID: ", groupID, " has no NPVR Provider configured."));
            //        case NPVRProvider.AlcatelLucent:
            //            res = new AlcatelLucentNPVR(groupID);
            //            break;
            //        case NPVRProvider.Kaltura:
            //        case NPVRProvider.Harmonic:
            //        default:
            //            throw new NotImplementedException(String.Concat("NPVR Provider not implemented. ID: ", npvrProviderID, " Name: ", provider.ToString()));
            //    }
            //}
            //else
            //{
            //    string msg = String.Concat("No NPVRProvider enum corresponds to: ", npvrProviderID);
            //    Logger.Logger.Log("Error", msg, FACTORY_LOG_FILE);
            //    throw new NotImplementedException(msg);
            //}
            NPVRProvider provider = NPVRProvider.None;
            if (IsGroupHaveNPVRImpl(groupID, ref provider))
            {
                switch (provider)
                {
                    case NPVRProvider.None:
                        throw new NotImplementedException(String.Concat("No NPVR Provider is configured for group: ", groupID));
                    case NPVRProvider.AlcatelLucent:
                        res = new AlcatelLucentNPVR(groupID);
                        break;
                    case NPVRProvider.Kaltura:
                    case NPVRProvider.Harmonic:
                    default:
                        throw new NotImplementedException(String.Concat("NPVR Provider: ", provider.ToString(), " is not implemented. Group ID: ", groupID));
                }
            }
            else
            {
                throw new NotImplementedException("Group has no NPVR provider configured.");
            }

            return res;
        }
    }
}
