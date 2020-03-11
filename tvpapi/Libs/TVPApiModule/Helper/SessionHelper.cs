using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Helper;
using TVPApi;

namespace TVPApiModule.Helper
{
    public class SessionHelperEx : SessionHelper
    {
        public static PlatformType Platform
        {
            get
            {
                PlatformType platform;
                GetValueFromSession<PlatformType>("Platform", out platform);
                return platform;
            }

            set
            {
                SetValueInSession("Platform", value);
            }
        }

        public static int GroupID
        {
            get
            {
                int iGroupID;
                GetValueFromSession<int>("GroupID", out iGroupID);
                return iGroupID;
            }

            set
            {
                SetValueInSession("GroupID", value);
            }
        }

        public static string WS_User
        {
            get
            {
                string sWSUser;
                GetValueFromSession<string>("WS_User", out sWSUser);
                return sWSUser;
            }

            set
            {
                SetValueInSession("WS_User", value);
            }
        }

        public static string WS_Pass
        {
            get
            {
                string sWSPass;
                GetValueFromSession<string>("WS_Pass", out sWSPass);
                return sWSPass;
            }

            set
            {
                SetValueInSession("WS_Pass", value);
            }
        }
    }
}
