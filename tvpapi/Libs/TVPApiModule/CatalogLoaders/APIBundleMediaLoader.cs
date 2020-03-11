using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Manager;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIBundleMediaLoader : BundleMediaLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get
            {
                return m_sCulture;
            }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        public int GroupIDParent
        {
            get;
            set;
        }

        #region Constructors
        //public APIBundleMediaLoader(int bundleId, int groupID, int groupIDParent, string platform, string userIP, string picSize) :
        //    this(bundleId, groupID, groupIDParent, platform, userIP, picSize)
        //{
        //}
        //int bundleId, string mediaType, OrderObj order, int groupID, string userIP, int pageSize, int pageIndex, string picSize
        public APIBundleMediaLoader(int bundleId, string mediaType, OrderObj order, int groupID, int groupIDParent, string platform, string userIP, string picSize, int pageIndex, int pageSize, eBundleType bundleType) :
            base(bundleId, mediaType, order, groupID, userIP, pageSize, pageIndex, picSize, bundleType)
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
