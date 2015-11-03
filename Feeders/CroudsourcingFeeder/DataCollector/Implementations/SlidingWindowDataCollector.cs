using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.Implementations;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.WS_Catalog;
using KLogMonitor;
using Tvinci.Core.DAL;
using OrderBy = ApiObjects.SearchObjects.OrderBy;

namespace CrowdsourcingFeeder.DataCollector.Implementations
{
    public class SlidingWindowDataCollector : BaseDataCollector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public SlidingWindowDataCollector(int assetId, int groupId)
            : base(assetId, groupId, eCrowdsourceType.SlidingWindow)
        {

        }

        protected override int[] Collect()
        {
            try
            {
                string catalogSignString = Guid.NewGuid().ToString();
                ChannelResponse slidingWindowResponse = (ChannelResponse)CatalogClient.GetResponse(new ChannelRequest()
                {
                    m_nChannelID = this.AssetId,
                    m_nGroupID = this.GroupId,
                    m_oFilter = new Filter()
                    {
                        m_bOnlyActiveMedia = true,
                        m_bUseStartDate = true,
                        m_bUseFinalDate = true,
                    },
                    m_sSignString = catalogSignString,
                    m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, TVinciShared.WS_Utils.GetTcmConfigValue("CatalogSignatureKey")),
                    m_nPageIndex = 0,
                    m_nPageSize = TVinciShared.WS_Utils.GetTcmIntValue("crowdsourcer.CATALOG_PAGE_SIZE"),
                });

                if (slidingWindowResponse != null && slidingWindowResponse.m_nMedias != null)
                {
                    return slidingWindowResponse.m_nMedias.Select(x => x.assetID).ToArray();
                }
                else return null;

            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector: {0} - Error collecting items - Exception: \n {1}", CollectorType, ex.Message), ex);
                return null;
            }
        }

        protected override Dictionary<int, BaseCrowdsourceItem> Normalize(SingularItem item)
        {
            try
            {
                Dictionary<int, BaseCrowdsourceItem> normalizedDictionary = null;

                string catalogSignString = Guid.NewGuid().ToString();
                long epochDateTime = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);

                // Get channel info
                ChannelObjResponse channelObjResponse = (ChannelObjResponse)CatalogClient.GetResponse(new ChannelObjRequest()
                    {
                        ChannelId = this.AssetId,
                        m_nGroupID = this.GroupId,
                        m_oFilter = new Filter()
                        {
                            m_bOnlyActiveMedia = true,
                            m_bUseStartDate = true,
                            m_bUseFinalDate = true,
                        },
                        m_sSignString = catalogSignString,
                        m_sSignature =
                            TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString,
                                TVinciShared.WS_Utils.GetTcmConfigValue("CatalogSignatureKey")),
                    });

                if (channelObjResponse != null && channelObjResponse.ChannelObj.m_OrderObject.m_bIsSlidingWindowField)
                {
                    //Get asset-stats
                    AssetStatsResponse assetStats = (AssetStatsResponse)CatalogClient.GetResponse(new AssetStatsRequest()
                    {
                        m_nGroupID = GroupId,
                        m_type = StatsType.MEDIA,
                        m_nAssetIDs = new[] { item.Id },
                        m_dStartDate = channelObjResponse.ChannelObj.m_OrderObject.m_dSlidingWindowStartTimeField,
                        m_dEndDate = DateTime.UtcNow,
                        m_sSignString = catalogSignString,
                        m_sSignature =
                            TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString,
                                TVinciShared.WS_Utils.GetTcmConfigValue("CatalogSignatureKey")),

                    });

                    // get language specific info
                    Dictionary<LanguageObj, MediaResponse> mediaInfoDict = GetLangAndInfo(GroupId, item.Id);
                    if (mediaInfoDict != null)
                    {
                        normalizedDictionary = new Dictionary<int, BaseCrowdsourceItem>();
                        foreach (KeyValuePair<LanguageObj, MediaResponse> mediaInfo in mediaInfoDict)
                        {
                            if (mediaInfo.Value.m_lObj[0] != null)
                            {
                                SlidingWindowItem croudsourceItem = new SlidingWindowItem
                                {
                                    Action = channelObjResponse.ChannelObj.m_OrderObject.m_eOrderBy,
                                    ActionDescription = channelObjResponse.ChannelObj.m_OrderObject.m_eOrderBy.ToString(),
                                    MediaId = item.Id,
                                    MediaName = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_sName,
                                    MediaImage =
                                        ((MediaObj)mediaInfo.Value.m_lObj[0]).m_lPicture.Select(
                                            pic => new BaseCrowdsourceItem.Pic()
                                            {
                                                Size = pic.m_sSize,
                                                URL = pic.m_sURL
                                            }).ToArray(),
                                    TimeStamp = epochDateTime,
                                    Order = item.Order,
                                    Period = channelObjResponse.ChannelObj.m_OrderObject.lu_min_period_id,
                                    PeriodDescription = GetMinPeriodDescription(channelObjResponse.ChannelObj.m_OrderObject.lu_min_period_id)
                                };

                                switch (croudsourceItem.Action)
                                {
                                    case OrderBy.VIEWS:
                                        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_nViews;
                                        break;
                                    case OrderBy.RATING:
                                        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_dRate;
                                        break;
                                    case OrderBy.LIKE_COUNTER:
                                        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_nLikes;
                                        break;
                                    case OrderBy.VOTES_COUNT:
                                        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_nVotes;
                                        break;
                                }
                                normalizedDictionary.Add(mediaInfo.Key.ID, croudsourceItem);
                            }
                        }
                    }
                }
                return normalizedDictionary;
            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector:{0} - Error normalizing singular item. mediaId {1} - Exception: \n {2}", CollectorType, item.Id, ex.Message), ex);
                return null;
            }
        }


        private static string GetMinPeriodDescription(int id)
        {
            string res = null;
            Dictionary<string, string> minPeriods;
            if (CachingManager.CachingManager.Exist("MinPeriods"))
            {
                minPeriods = CachingManager.CachingManager.GetCachedData("MinPeriods") as Dictionary<string, string>;
            }
            else
            {
                minPeriods = CatalogDAL.GetMinPeriods();
                if (minPeriods != null)
                    CachingManager.CachingManager.SetCachedData("MinPeriods", minPeriods, 604800, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            if (minPeriods != null)
                minPeriods.TryGetValue(id.ToString(), out res);

            return res;
        }
    }
}
