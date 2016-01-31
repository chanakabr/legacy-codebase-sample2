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
        public APIExternalSearchMediaLoader(string query, List<int> mediaTypes, int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex)
            : base(query, mediaTypes, groupID, userIP, pageSize, pageIndex)
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

        protected override object Process()
        {
            // response should be media ids with status, try to save it and override just the obj.
            MediaIdsStatusResponse response = m_oResponse as MediaIdsStatusResponse;
            if (response == null)
                return base.Process();
            else
            {
                response.m_lObj = (List<BaseObject>)base.Process();
                m_oResponse = response;
            }

            return m_oResponse != null ? m_oResponse.m_lObj : null;
        }
    }
}
