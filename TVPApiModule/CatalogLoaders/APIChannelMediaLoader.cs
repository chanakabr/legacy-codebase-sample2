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
using TVPApiModule.Manager;
using TVPApiModule.Helper;

namespace TVPApiModule.CatalogLoaders
{
    public class APIChannelMediaLoader : ChannelMediaLoader
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
        public APIChannelMediaLoader(int channelID, int groupID, PlatformType platform, string udid, string userIP, int pageSize, int pageIndex, string picSize, string language, List<KeyValue> tagsMetas = null, CutWith cutWith = Tvinci.Data.Loaders.TvinciPlatform.Catalog.CutWith.AND)
            : base(channelID, groupID, userIP, pageSize, pageIndex, picSize, tagsMetas, cutWith)
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

            //FlashVars techConfigFlashVars = ConfigManager.GetInstance().GetConfig(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).TechnichalConfiguration.Data.TVM.FlashVars;
            //string fileFormat = techConfigFlashVars.FileFormat;
            //string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();
            //dsItemInfo retVal = CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);
            //dsItemInfo.ChannelRow channelRow = retVal.Channel.NewChannelRow();
            //ChannelResponse response = m_oResponse as ChannelResponse;
            //channelRow.ChannelId = response.Id.ToString();
            //channelRow.Title = response.m_sName;
            //channelRow.Description = response.m_sDescription;
            //channelRow.EnableRssFeed = response.m_sEnableRssFeed == 1 ? true : false;
            //retVal.Channel.AddChannelRow(channelRow);
            //return retVal;        
        }
    }
}
