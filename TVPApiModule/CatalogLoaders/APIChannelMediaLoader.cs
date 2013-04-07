using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPApi;
using System.Data;
using TVPPro.Configuration.Technical;

namespace TVPApiModule.CatalogLoaders
{
    public class APIChannelMediaLoader : ChannelMediaLoader
    {
        public int GroupIDParent { get; set; }

        #region Constructors
        public APIChannelMediaLoader(int channelID, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize)
            : base(channelID, groupID, userIP, pageSize, pageIndex, picSize)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
        }
        public APIChannelMediaLoader(int channelID, int groupID, int groupIDParent, string userIP, int pageSize, int pageIndex, string picSize, int language)
            : this(channelID, groupID, groupIDParent, userIP, pageSize, pageIndex, picSize)
        {
            Language = language;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            FlashVars techConfigFlashVars = ConfigManager.GetInstance().GetConfig(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).TechnichalConfiguration.Data.TVM.FlashVars;
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            dsItemInfo retVal = CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
            dsItemInfo.ChannelRow channelRow = retVal.Channel.NewChannelRow();
            ChannelResponse response = m_oResponse as ChannelResponse;
            channelRow.ChannelId = response.Id.ToString();
            channelRow.Title = response.m_sName;
            channelRow.Description = response.m_sDescription;
            channelRow.EnableRssFeed = response.m_sEnableRssFeed == 1 ? true : false;
            retVal.Channel.AddChannelRow(channelRow);
            return retVal;        
        }
    }
}
