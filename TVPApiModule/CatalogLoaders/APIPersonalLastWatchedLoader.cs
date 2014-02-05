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
using TVPApiModule.Manager;
using TVPApiModule.Helper;
using TVPApiModule.Context;

namespace TVPApiModule.CatalogLoaders
{
    public class APIPersonalLastWatchedLoader : PersonalLastWatchedLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }


        #region Constructors
        public APIPersonalLastWatchedLoader(string siteGuid, int groupID, PlatformType platform, string udid, string userIP, string language, int pageSize, int pageIndex, string picSize)
            : base(siteGuid, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            Platform = platform.ToString();
            DeviceId = udid;
            Culture = language;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            return APICatalogHelper.MediaObjToMedias(medias, PicSize, m_oResponse.m_nTotalItems, GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform));
        }

    }
}
