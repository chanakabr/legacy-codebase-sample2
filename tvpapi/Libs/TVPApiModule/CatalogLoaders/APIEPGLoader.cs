using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Manager;
using TVPPro.SiteManager.CatalogLoaders;

namespace TVPApiModule.CatalogLoaders
{
    [Serializable]
    public class APIEPGLoader : EPGLoader
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

        public APIEPGLoader(int groupID, string platform, string userIP, int pageSize, int pageIndex, List<int> channelIDs, EpgSearchType searchType, DateTime startTime, DateTime endTime, int nextTop, int prevTop, string language)
            : base(groupID, userIP, pageSize, pageIndex, channelIDs, searchType, startTime, endTime, nextTop, prevTop)
        {
            Platform = platform;
            Culture = language;
        }
        #endregion
    }
}
