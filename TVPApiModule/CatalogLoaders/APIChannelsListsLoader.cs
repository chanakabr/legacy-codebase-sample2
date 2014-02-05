using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.CatalogLoaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.DataEntities;
using TVPPro.Configuration.Technical;
using TVPApi;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;
using TVPApiModule.Helper;
using TVPApiModule.Context;

namespace TVPApiModule.CatalogLoaders
{
    public class APIChannelsListsLoader : ChannelsListsLoader
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

        public int GroupIDParent { get; set; }

        #region Constructors
        public APIChannelsListsLoader(int categoryID, int groupID, PlatformType platform, string udid, string userIP, string language, int pageSize, int pageIndex, string picSize)
            : base(categoryID, groupID, userIP, pageSize, pageIndex, picSize)
        {
            Platform = platform.ToString();
            DeviceId = udid;
            Culture = language;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<channelObj> channels)
        {
            return APICatalogHelper.ChannelObjToChannel(channels, PicSize);
        }
    }
}
