using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using System.Data;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class ChannelMediaLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string userIP;
        private List<KeyValue> tagsMetas;
        private CutWith cutWith;

        public int ChannelID { get; set; }
        public OrderObj OrderObj { get; set; }

        #region Constructors
        public ChannelMediaLoader(int channelID, int groupID, string userIP, int pageSize, int pageIndex, string picSize, OrderObj orderObj)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            ChannelID = channelID;
            OrderObj = orderObj;
        }

        public ChannelMediaLoader(int channelID, string userName, string userIP, int pageSize, int pageIndex, string picSize, OrderObj orderObj)
            : this(channelID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize, orderObj)
        {
        }

        public ChannelMediaLoader(int channelID, int groupID, string userIP, int pageSize, int pageIndex, string picSize, List<KeyValue> tagsMetas, CutWith cutWith, OrderObj orderObj)
            : this(channelID, groupID, userIP, pageSize, pageIndex, picSize, orderObj)
        {
            // TODO: Complete member initialization
            this.ChannelID = channelID;
            this.GroupID = groupID;
            this.userIP = userIP;
            this.PageSize = pageSize;
            this.PageIndex = pageIndex;
            this.PicSize = picSize;
            this.tagsMetas = tagsMetas;
            this.cutWith = cutWith;
            this.OrderObj = orderObj;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new ChannelRequestMultiFiltering()
            {
                m_nChannelID = ChannelID,
                m_oOrderObj = OrderObj,
                m_eFilterCutWith = cutWith,
                m_lFilterTags = tagsMetas,
                m_bAddDeviceRuleID = true
            };
        }

        protected override object Process()
        {
            string cacheKey = GetLoaderCachekey();
            if (m_oResponse != null && ((ChannelResponse)m_oResponse).m_nMedias != null && ((ChannelResponse)m_oResponse).m_nMedias.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);
                m_oMediaCache = new MediaCache(((ChannelResponse)m_oResponse).m_nMedias, GroupID, m_sUserIP, m_oFilter);
            }
            else if (m_oResponse == null)// No Response from Catalog, gets medias from cache
            {
                m_oResponse = CacheManager.Cache.GetFailOverResponse(cacheKey);
                if (m_oResponse != null && ((ChannelResponse)m_oResponse).m_nMedias != null && ((ChannelResponse)m_oResponse).m_nMedias.Count > 0)
                {
                    m_oMediaCache = new MediaCache(((ChannelResponse)m_oResponse).m_nMedias, GroupID, m_sUserIP, m_oFilter);
                }
            }
            if (m_oMediaCache != null)
            {
                m_oMediaCache.BuildRequest();
                m_oResponse.m_lObj = (List<BaseObject>)m_oMediaCache.Execute();
            }
            return m_oResponse != null ? m_oResponse.m_lObj : null;
        }

        public override string GetLoaderCachekey()
        {
            return string.Format("channel{0}_index{1}_size{2}_group{3}", ChannelID, PageIndex, PageSize, GroupID);
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.ChannelRequest":
                        ChannelRequest channelRequest = obj as ChannelRequest;
                        sText.AppendFormat("ChannelRequest: ChannelID = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", channelRequest.m_nChannelID, channelRequest.m_nGroupID, channelRequest.m_nPageIndex, channelRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.ChannelResponse":
                        ChannelResponse channelResponse = obj as ChannelResponse;
                        sText.AppendFormat("ChannelResponse: ChannelID = {0}, ChannelName = {1}, TotalItems = {2}, ", channelResponse.Id, channelResponse.m_sName, channelResponse.m_nTotalItems);
                        sText.AppendLine(channelResponse.m_nMedias.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
            //logger.Info(sText.ToString());
        }
    }
}
