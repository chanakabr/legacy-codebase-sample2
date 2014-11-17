using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPVR
{
    /*
     * Don't change the visibility of this class to public. Any communication with Third Part NPVR Providers should be done via
     * the interface INPVRProvider.
     */ 
    internal class AlcatelLucentNPVR : INPVRProvider
    {
        private static readonly string ALU_LOG_FILE = "AlcatelLucent";
        private static readonly string LOG_HEADER_EXCEPTION = "Exception";

        private int groupID;

        public AlcatelLucentNPVR(int groupID)
        {
            this.groupID = groupID;
        }

        private bool IsCreateInputValid(NPVRParamsObj args)
        {
            long domainID = 0;
            return args != null && !string.IsNullOrEmpty(args.EntityID) && Int64.TryParse(args.EntityID, out domainID) && domainID > 0;
        }

        public bool CreateAccount(NPVRParamsObj args)
        {
            try
            {
                if (IsCreateInputValid(args))
                {
                    Logger.Logger.Log("Create", string.Format("Create request has been issued. G ID: {0} , Params Obj: {1}", groupID, args.ToString()), ALU_LOG_FILE);
                }
                else
                {
                    throw new ArgumentException("Either args obj is null or domain id is empty.");
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log(LOG_HEADER_EXCEPTION, GetLogMsg("Exception at Create.", args, ex), ALU_LOG_FILE);
                throw;
            }

            return false;
        }

        public bool DeleteAccount(NPVRParamsObj args)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }

            return false;
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
