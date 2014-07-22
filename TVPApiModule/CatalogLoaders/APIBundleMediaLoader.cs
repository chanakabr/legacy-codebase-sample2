using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Context;
using TVPApiModule.Helper;
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
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }

        public int GroupIDParent { get; set; }

        #region Constructors

        public APIBundleMediaLoader(int bundleId, string mediaType, OrderObj order, int groupID, int groupIDParent, PlatformType platform, string userIP, string language, string picSize, int pageIndex, int pageSize, CatalogBundleType bundleType) :
            base(bundleId, mediaType, order, groupID, userIP, pageSize, pageIndex, picSize, bundleType)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform.ToString();
            Culture = language;
        }

        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            return APICatalogHelper.MediaObjToMedias(medias, PicSize, m_oResponse.m_nTotalItems, GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform));            
        }
    }
}
