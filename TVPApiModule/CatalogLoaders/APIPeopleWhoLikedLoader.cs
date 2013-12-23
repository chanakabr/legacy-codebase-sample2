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
using TVPApiModule.Manager;
using TVPApiModule.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIPeopleWhoLikedLoader : PeopleWhoLikedLoader
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
        public APIPeopleWhoLikedLoader(int mediaID, int mediaFileID, int countryID, int socialAction, int socialPlatform, int groupID, string platform, string udid, string userIP, string language, int pageSize, int pageIndex, string picSize)
            : base(mediaID, mediaFileID, countryID, socialAction, socialPlatform, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            Platform = platform;
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
