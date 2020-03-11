using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Manager;
using TVPPro.SiteManager.CatalogLoaders;

namespace TVPApiModule.CatalogLoaders
{
    [Serializable]
    public class APIEPGSearchLoader : EPGSearchLoader
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

        public APIEPGSearchLoader(int groupID, string platform, string userIP, int pageSize, int pageIndex, string searchText, DateTime startTime, DateTime endTime)
            : base(groupID, userIP, pageSize, pageIndex, searchText, startTime, endTime)
        {
            Platform = platform;
        }

        public APIEPGSearchLoader(int groupID, string platform, string userIP, int pageSize, int pageIndex, List<KeyValue> andList, List<KeyValue> orList, bool exact, DateTime startTime, DateTime endTime)
            : base(groupID, userIP, pageSize, pageIndex, andList, orList, exact, startTime, endTime)
        {
            Platform = platform;
        }
        #endregion

    }
}
