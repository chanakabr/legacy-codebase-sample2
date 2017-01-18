using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ApiObjects.ConditionalAccess;

namespace Core.ConditionalAccess
{
    public abstract class BaseNPVRCommand
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected static readonly string NPVR_LOG_FILE = "NPVR";
        protected static readonly string LOG_HEADER_EXCEPTION = "Exception";
        public string wsUsername;
        public string wsPassword;
        public string siteGuid;
        public long domainID;
        public string udid;
        public string assetID; // may be either ALU Series ID or Tvinci EPG ID.

        public NPVRResponse Execute()
        {
            NPVRResponse res = null;
            try
            {
                BaseConditionalAccess t = null;
                int groupID = Utils.GetGroupID(wsUsername, wsPassword, "GetNPVRResponse", ref t);
                if (groupID == 0 || t == null)
                {
                    return new NPVRResponse() { status = NPVRStatus.BadRequest.ToString() };
                }

                res = ExecuteFlow(t);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at Execute. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Req: ", ToString()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error(LOG_HEADER_EXCEPTION + sb.ToString(), ex);
                res = new NPVRResponse() { status = NPVRStatus.Error.ToString() };
            }

            return res;

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat("this is: ", this.GetType().Name));
            sb.Append(String.Concat(" Site Guid: ", siteGuid));
            sb.Append(String.Concat(" Asset ID: ", assetID));
            sb.Append(String.Concat(" UDID: ", udid));
            sb.Append(String.Concat(" Domain ID: ", domainID));
            return sb.ToString();
        }

        protected abstract NPVRResponse ExecuteFlow(BaseConditionalAccess cas);

    }
}
