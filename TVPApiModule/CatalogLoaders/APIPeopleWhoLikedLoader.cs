using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Helper;
using TVPApi;

namespace TVPApiModule.CatalogLoaders
{
    public class APIPeopleWhoLikedLoader : PeopleWhoLikedLoader
    {
        public int GroupIDParent { get; set; }

        #region Constructors
        public APIPeopleWhoLikedLoader(int mediaID, int mediaFileID, int countryID, int socialAction, int socialPlatform, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize)
            : base(mediaID, mediaFileID, countryID, socialAction, socialPlatform, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
        }

        public APIPeopleWhoLikedLoader(int mediaID, int mediaFileID, int countryID, int socialAction, int socialPlatform, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, int language)
            : this(mediaID, mediaFileID, countryID, socialAction, socialPlatform, groupID, groupIDParent, userIP, pageSize, pageIndex, picSize)
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
