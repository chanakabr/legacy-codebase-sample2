using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public interface ISSOProviderImplementation
    {
        UserResponseObject SignIn(string wsUN, string wsPass, string sCoGuid, string sPass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins);
        UserResponseObject CheckLogin(string sUserName, int nOperatorID);
    }
}
