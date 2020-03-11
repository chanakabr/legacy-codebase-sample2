using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects
{
    public class FBConnectObj
    {
        public string RegStatus { get; set; }
        public string LoginStatus { get; set; }
        public string SiteGuid { get; set; }

        public FBConnectObj()
        {
            
        }

        public FBConnectObj(eRegResp regStatus, eLoginResp loginStatus, string sSiteguid)
        {
            RegStatus = regStatus.ToString();
            LoginStatus = loginStatus.ToString();
        }

        public enum eLoginResp
        {
            OK = 1,
            Error = 2
        }

        public enum eRegResp
        {
            OK = 1,
            Error = 2
        }
    }

    public class FBConnectConfig
    {
        public string appId { get; set; }
        public string scope { get; set; }
        public string apiUser { get; set; }
        public string apiPass { get; set; }
    }

    public enum eLoginResp
    {
        OK = 1,
        Error = 2
    }

    public enum eRegResp
    {
        OK = 1,
        Error = 2
    }

    public enum eFBAction
    {
        check = 0
    }
}
