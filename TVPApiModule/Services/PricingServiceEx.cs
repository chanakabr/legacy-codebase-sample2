using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;
using TVPApi;

namespace TVPApiModule.Services
{
    public class PricingServiceEx : PricingService
    {
        public PricingServiceEx(int groupID, string platform) : base(groupID, platform)
        {
            base.m_Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule();
            base.m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.URL;
            base.wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultUser;
            base.wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultPassword;
        }
    }
}
