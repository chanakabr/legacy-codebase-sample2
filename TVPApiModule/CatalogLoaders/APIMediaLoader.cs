using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPPro.SiteManager.CatalogLoaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.DataEntities;
using TVPApi;
using TVPPro.Configuration.Technical;

namespace TVPApiModule.CatalogLoaders
{
    public class APIMediaLoader : MediaLoader
    {
        public int GroupIDParent { get; set; }

        #region Constructors
        public APIMediaLoader(int mediaID, int groupID, int groupIDParent, string userIP, string picSize) :
            this(new List<int>() { mediaID }, groupID, groupIDParent, userIP, picSize)
        {
        }

        public APIMediaLoader(List<int> mediaIDs, int groupID, int groupIDParent, string userIP, string picSize) :
            base(mediaIDs, groupID, userIP, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;

        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            FlashVars techConfigFlashVars = ConfigManager.GetInstance().GetConfig(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).TechnichalConfiguration.Data.TVM.FlashVars;
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            return CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
        }
    }
}
