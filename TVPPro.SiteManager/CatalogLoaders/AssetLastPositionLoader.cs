using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Objects;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class AssetLastPositionLoader : CatalogRequestManager, ILoaderAdapter
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string UDID { get; set; }
        public string AssetID { get; set; }
        public int MyProperty { get; set; }
    }
}
