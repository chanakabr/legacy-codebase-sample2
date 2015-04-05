using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class FBConnectObj
    {
        public string reg_status { get; set; }
        public string login_status { get; set; }
        public string site_guid { get; set; }

        public FBConnectObj()
        {
            
        }

        public FBConnectObj(eRegResp regStatus, eLoginResp loginStatus, string sSiteguid)
        {
            reg_status = regStatus.ToString();
            login_status = loginStatus.ToString();
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
        public string app_id { get; set; }
        public string scope { get; set; }
        public string api_user { get; set; }
        public string api_pass { get; set; }
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
