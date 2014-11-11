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
                throw new Exception("Either username or password is incorrect.");
            }
            return ExecuteFlow(t);

        }

        protected abstract NPVRResponse ExecuteFlow(BaseConditionalAccess cas);
    }
}
