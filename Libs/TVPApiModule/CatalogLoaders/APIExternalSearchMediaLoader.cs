using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.DataEntities;
using TVPApi;
using TVPPro.Configuration.Technical;
using TVPApiModule.Manager;

namespace TVPApiModule.CatalogLoaders
{
    class APIExternalSearchMediaLoader : ExternalSearchMediaLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        public int GroupIDParent { get; set; }

        #region Constructors
        public APIExternalSearchMediaLoader(string query, List<int> mediaTypes, int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex, string picSize)
            : base(query, mediaTypes, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform;
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
