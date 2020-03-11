using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public interface ISSOProvider
    {
        UserResponseObject SignIn(string sCoGuid, string sPass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins);
        UserResponseObject CheckLogin(string sUserName, int nOperatorID);
    }
}
