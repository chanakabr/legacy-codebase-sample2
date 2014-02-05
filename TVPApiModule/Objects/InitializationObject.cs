using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using TVPApiModule.Context;

/// <summary>
/// Summary description for InitializationObject
/// </summary>
/// 

namespace TVPApiModule.Objects
{
    [Serializable]
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

        public InitializationObject()
        {

        }
    }

    [Serializable]
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
}
