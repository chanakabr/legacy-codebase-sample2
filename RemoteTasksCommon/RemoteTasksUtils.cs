using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TasksCommon
{
    public class RemoteTasksUtils
    {
        public static void GetCredentials(int groupId, ref string wsUserName, ref string wsPassword, ApiObjects.eWSModules subModule)
        {
            ApiObjects.Credentials credtentials = TvinciCache.WSCredentials.GetWSCredentials(ApiObjects.eWSModules.REMOTETASK, groupId, subModule);

            if (credtentials != null)
            {
                wsUserName = credtentials.m_sUsername;
                wsPassword = credtentials.m_sPassword;
            }
        }
    }
}
