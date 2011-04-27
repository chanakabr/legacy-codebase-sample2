using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;
using TVPApi;

namespace TVPApiModule.Services
{
    public class ConditionalAccessServiceEx : ConditionalAccessService
    {
        public ConditionalAccessServiceEx(int groupID, string platform)
            : base(groupID, platform)
        {
            base.m_Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module();
            base.m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.URL;
            base.wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            base.wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
        }
    }
}
