using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for InitializationObject
/// </summary>
/// 

namespace TVPApi
{
    public class InitializationObject
    {
        //User locale object
        public Locale Locale { get; set; }
        //User Platform
        public PlatformType Platform { get; set; }

        public string SiteGuid { set; get; }
        public int DomainID { set; get; }
        public string UDID { set; get; }

        public string ApiUser { get; set; }
        public string ApiPass { get; set; }

        public string Token { get; set; }

        public InitializationObject()
        {

        }
    }

    public class Locale
    {
        public string LocaleLanguage { get; set; }
        public string LocaleCountry { get; set; }
        public string LocaleDevice { get; set; }
        public TVPApi.LocaleUserState LocaleUserState { get; set; }

        public Locale()
        {

        }
    }
}
