using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPApi;
using System.Data;
using TVPPro.Configuration.Technical;
using TVPApiModule.Manager;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Response;

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
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupIDParent, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }
        
        public int GroupIDParent { get; set; }
        

        #region Constructors
        public APIChannelMediaLoader(int channelID, int groupID, int groupIDParent, string platform, string userIP, int pageSize, int pageIndex, OrderObj orderObj, string picSize, List<KeyValue> tagsMetas = null, CutWith cutWith = CutWith.AND)
            : base(channelID, groupID, userIP, pageSize, pageIndex, picSize, tagsMetas, cutWith, orderObj)
        {
            overrideExecuteAdapter += ApiExecuteMultiMediaAdapter;
            GroupIDParent = groupIDParent;
            Platform = platform;
            OrderObj = orderObj;
        }
        #endregion

        public object ApiExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            var techConfigFlashVars = GroupsManager.GetGroup(GroupIDParent).GetFlashVars((PlatformType)Enum.Parse(typeof(PlatformType), Platform));
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
