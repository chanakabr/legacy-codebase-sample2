using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using TVPApiModule.Manager;
using TVPPro.SiteManager.CatalogLoaders;

namespace TVPApiModule.CatalogLoaders
{
    [Serializable]
    public class APIEPGSearchContentLoader : EPGSearchContentLoader
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

        public APIEPGSearchContentLoader(int groupID, string platform, string userIP, int pageSize, int pageIndex, string searchText, string language)
            : base(groupID, userIP, pageSize, pageIndex, searchText)
        {
            Platform = platform;
            Culture = language;
        }
        #endregion
    }
}
