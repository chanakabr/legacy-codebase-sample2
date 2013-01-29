using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.TvinciPlatform.Social;
using System.Configuration;


namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class SocialService : System.Web.Services.WebService, ISocialService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(SiteService));
      

    }
}
