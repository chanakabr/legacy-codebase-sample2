using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConditionalAccess
{
    public abstract class BaseNPVRCommand
    {
        public string wsUsername;
        public string wsPassword;
        public string siteGuid;

        public NPVRResponse Execute()
        {
            BaseConditionalAccess t = null;
            int groupID = Utils.GetGroupID(wsUsername, wsPassword, "GetNPVRResponse", ref t);
            if (groupID == 0 || t == null)
            {
                return new NPVRResponse() { domainID = 0, status = NPVRStatus.BadRequest.ToString() };
            }
            int domainID = 0;
            if (!Utils.IsUserValid(siteGuid, groupID, ref domainID))
            {
                return new NPVRResponse() { domainID = domainID, status = NPVRStatus.BadRequest.ToString() };
            }

            return ExecuteFlow(t, domainID);

        }

        protected abstract NPVRResponse ExecuteFlow(BaseConditionalAccess cas, int domainID);

    }
}
