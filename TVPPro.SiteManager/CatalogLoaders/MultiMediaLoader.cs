using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.DataEntities;
using System.Data;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.Manager;
using TVPPro.Configuration.Technical;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.Helper;
using TVPPro.Configuration.PlatformServices;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public abstract class MultiMediaLoader : CatalogRequestManager, ILoaderAdapter, ISupportPaging
    {
        protected delegate object ExecuteMediaAdapter(List<BaseObject> medias);
        protected ExecuteMediaAdapter overrideExecuteAdapter;
        public string PicSize { get; set; }

        protected MediaCache m_oMediaCache;

        #region Constructors
        public MultiMediaLoader(int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex)
        {
            PicSize = picSize;
        }

        public MultiMediaLoader(string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }

        public MultiMediaLoader(int groupID, string userIP, int pageSize, int pageIndex, string picSize, Provider provider)
            : this(groupID, userIP, pageSize, pageIndex, picSize)
        {
            m_oProvider = provider;
        }
        #endregion

        // for failover support
        public virtual string GetLoaderCachekey()
        {
            return null;
        }

        protected virtual object Process()
        {
            string cacheKey = GetLoaderCachekey();

            if (m_oResponse is UnifiedSearchResponse)
            {
                UnifiedSearchResponse usr = (m_oResponse as UnifiedSearchResponse);
                MediaIdsStatusResponse newResp = new MediaIdsStatusResponse()
                {
                    ExtensionData = usr.ExtensionData,
                    m_lObj = usr.m_lObj,
                    Status = usr.status,
                    m_nTotalItems = usr.m_nTotalItems,
                    m_nMediaIds = usr.searchResults.Select(x => new SearchResult() { assetID = int.Parse(x.AssetId), ExtensionData = x.ExtensionData, UpdateDate = x.m_dUpdateDate }).ToList()
                };
                m_oResponse = newResp;
            }

            if (m_oResponse != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds.Count > 0)
            {
                CacheManager.Cache.InsertFailOverResponse(m_oResponse, cacheKey);
                m_oMediaCache = new MediaCache(((MediaIdsResponse)m_oResponse).m_nMediaIds, GroupID, m_sUserIP, m_oFilter);
            }
            else if (m_oResponse == null)// No Response from Catalog, gets medias from cache
            {
                m_oResponse = CacheManager.Cache.GetFailOverResponse(cacheKey);
                if (m_oResponse != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds.Count > 0)
                {
                    m_oMediaCache = new MediaCache(((MediaIdsResponse)m_oResponse).m_nMediaIds, GroupID, m_sUserIP, m_oFilter);
                }
            }
            if (m_oMediaCache != null)
            {
                m_oMediaCache.BuildRequest();
                m_oResponse.m_lObj = (List<BaseObject>)m_oMediaCache.Execute();
            }
            return m_oResponse != null ? m_oResponse.m_lObj : null;

        }

        public virtual object Execute()
        {
            object retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            List<BaseObject> lObj = null;

            m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse);
            {
                Log("Got:", m_oResponse);
                lObj = (List<BaseObject>)Process();
            }
            if (lObj != null)
            {
                retVal = ExecuteMultiMediaAdapter(lObj);
            }
            else
            {
                retVal = new dsItemInfo();
            }
            return retVal;

        }

        protected virtual object ExecuteMultiMediaAdapter(List<BaseObject> medias)
        {
            dsItemInfo retVal;
            if (overrideExecuteAdapter != null)
            {
                retVal = overrideExecuteAdapter(medias) as dsItemInfo;
            }
            else
            {
                string fileFormat = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
                string subFileFormat = (TechnicalConfiguration.Instance.Data.TVM.FlashVars.SubFileFormat.Split(';')).FirstOrDefault();
                retVal = CatalogHelper.MediaObjToDsItemInfo(medias, PicSize, fileFormat, subFileFormat);

                // If Channel - Add Channel data
                if (m_oResponse is ChannelResponse)
                {
                    dsItemInfo.ChannelRow channelRow = retVal.Channel.NewChannelRow();
                    ChannelResponse response = m_oResponse as ChannelResponse;
                    channelRow.ChannelId = response.Id.ToString();
                    channelRow.Title = response.m_sName;
                    channelRow.Description = response.m_sDescription;
                    channelRow.EnableRssFeed = response.m_sEnableRssFeed == 1 ? true : false;
                    retVal.Channel.AddChannelRow(channelRow);
                }                
            }
            return retVal;
        }

        #region ISupportPaging method
        public bool TryGetItemsCount(out long count)
        {
            count = 0;

            if (m_oResponse == null)
                return false;

            count = m_oResponse.m_nTotalItems;

            return true;
        }
        #endregion

        #region ILoaderAdapter not implemented methods
        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute(eExecuteBehaivor behaivor)
        {
            throw new NotImplementedException();
        }

        public object LastExecuteResult
        {
            get { throw new NotImplementedException(); }
        }
        #endregion



    }
}
