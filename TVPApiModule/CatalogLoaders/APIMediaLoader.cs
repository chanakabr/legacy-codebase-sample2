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
using TVPApiModule.Manager;
using TVPApiModule.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIMediaLoader : MediaLoader
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
        public APIMediaLoader(int mediaID, int groupID, PlatformType platform, string udid, string userIP, string picSize, string language) :
            this(new List<int>() { mediaID }, groupID, platform, udid, userIP, picSize, language)
        {
        }

        public APIMediaLoader(List<int> mediaIDs, int groupID, PlatformType platform, string udid, string userIP, string picSize, string language) :
            base(mediaIDs, groupID, userIP, picSize)
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
