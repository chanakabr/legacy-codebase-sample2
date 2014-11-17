using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    /*
     * Don't change the visibility of this class to public. Any communication with a third party NPVR Provider should be done via
     * the interface INPVRProvider.
     */ 
    internal class AlcatelLucentNPVR : INPVRProvider
    {
        private static readonly string ALU_LOG_FILE = "AlcatelLucent";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";

        private static readonly string ENDPOINT_RECORD = "Record";
        private static readonly string ENDPOINT_SEASON = "Season"; // Series
        private static readonly string ENDPOINT_USER = "User";

        private int groupID;

        public AlcatelLucentNPVR(int groupID)
        {
            this.groupID = groupID;
        }

        private bool IsCreateInputValid(NPVRParamsObj args)
        {
            long domainID = 0;
            return args != null && args.Quota > 0 && !string.IsNullOrEmpty(args.EntityID) && Int64.TryParse(args.EntityID, out domainID) && domainID > 0;
        }

        public NPVRUserActionResponse CreateAccount(NPVRParamsObj args)
        {
            NPVRUserActionResponse res = new NPVRUserActionResponse();
            try
            {
                if (IsCreateInputValid(args))
                {
                    Logger.Logger.Log("Create", string.Format("Create request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()), ALU_LOG_FILE);

                }
                else
                {
                    throw new ArgumentException("Either args obj is null or domain id is empty or quota is non-positive.");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at Create.", args, ex), ALU_LOG_FILE);
                throw;
            }

            return res;
        }

        public NPVRUserActionResponse DeleteAccount(NPVRParamsObj args)
        {
            NPVRUserActionResponse res = new NPVRUserActionResponse();
            try
            {

            }
            catch (Exception ex)
            {

            }

            return res;
        }

        private long MinutesToSeconds(long quotaInMinutes)
        {
            return 60 * quotaInMinutes;
        }

        private string GetLogMsg(string msg, NPVRParamsObj obj, Exception ex)
        {
            StringBuilder sb = new StringBuilder(String.Concat(msg, "."));
            sb.Append(String.Concat(" Params Obj: ", obj != null ? obj.ToString() : "null"));
            sb.Append(String.Concat(" Group ID: ", groupID));
            if (ex != null)
            {
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
            }

            return sb.ToString();
        }

    }
}
