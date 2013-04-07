using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.CatalogLoaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Technical;
using TVPApi;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIChannelsListsLoader : ChannelsListsLoader
    {
        public int GroupIDParent { get; set; }

        #region Constructors
        public APIChannelsListsLoader(int categoryID, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize)
            : base(categoryID, groupID, userIP, pageSize, pageIndex, picSize)
        {
            GroupIDParent = groupIDParent;
        }
        public APIChannelsListsLoader(int categoryID, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, int language)
            : this (categoryID, groupID, groupIDParent, userIP, pageSize, pageIndex, picSize)
        {
            Language = language;
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
