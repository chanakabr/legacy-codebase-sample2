using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIUserSocialMediaLoader : UserSocialMediaLoader
    {
        public int GroupIDParent { get; set; }

        #region Constructors
        public APIUserSocialMediaLoader(string siteGuid, int socialAction, int socialPlatform, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize)
            : base(siteGuid, socialAction, socialPlatform, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
        }

        public APIUserSocialMediaLoader(string siteGuid, int socialAction, int socialPlatform, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, int language)
            : this(siteGuid, socialAction, socialPlatform, groupID, groupIDParent, userIP, pageSize, pageIndex, picSize)
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
