using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public delegate Dictionary<string, string> OmnitureDynamicData();

namespace TVPPro.SiteManager.Manager
{
    public class OmnitureManager
    {
        public OmnitureDynamicData OmnitureDelegate { get; set; }
        public bool IsWriteOmnitureJS { get; set; }

        public OmnitureManager(OmnitureDynamicData delegateFuncForOmniture)
        {
            IsWriteOmnitureJS = true;
            OmnitureDelegate = delegateFuncForOmniture;

        }

        public OmnitureManager(OmnitureDynamicData delegateFuncForOmniture, bool isWriteJS)
        {
            IsWriteOmnitureJS = isWriteJS;
            OmnitureDelegate = delegateFuncForOmniture;
        }
    }


}
