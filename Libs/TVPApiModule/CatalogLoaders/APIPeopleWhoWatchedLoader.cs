using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;
using ApiObjects;
using ApiObjects.Response;
using TVPApi;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;

namespace TVPApiModule.CatalogLoaders
{
    public class APIPeopleWhoWatchedLoader : PeopleWhoWatchedLoader
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
        public APIPeopleWhoWatchedLoader(int mediaID, int countryID, int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex, string picSize)
            : base(mediaID, countryID, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            var techConfigFlashVars = GroupsManager.GetGroup(GroupIDParent).GetFlashVars((PlatformType)Enum.Parse(typeof(PlatformType), Platform));
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            return CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
        }
    }
}
