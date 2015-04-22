using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects
{
    public class InitializationObject
    {
        //User locale object
        public Locale Locale { get; set; }
        //User Platform
        public RestfulTVPApi.Objects.Enums.PlatformType Platform { get; set; }

        public string SiteGuid { set; get; }
        public int DomainID { set; get; }
        public string UDID { set; get; }

        public string ApiUser { get; set; }
        public string ApiPass { get; set; }

        public InitializationObject()
        {

        }
    }

    public class Locale
    {
        public string LocaleLanguage { get; set; }
        public string LocaleCountry { get; set; }
        public string LocaleDevice { get; set; }
        public LocaleUserState LocaleUserState { get; set; }

        public Locale()
        {

        }
    }

    public enum LocaleUserState
    {
        Unknown = 0,
        Anonymous = 1,
        New = 2,
        Sub = 3,
        ExSub = 4,
        PPV = 5,
        ExPPV = 6
    }
}