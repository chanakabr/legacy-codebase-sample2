using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.Implementations;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.WS_Catalog;
using Tvinci.Core.DAL;
using OrderBy = ApiObjects.SearchObjects.OrderBy;

namespace CrowdsourcingFeeder.DataCollector.Implementations
{
    public class SlidingWindowDataCollector : BaseDataCollector
    {
        private Channel _channelData;

        public SlidingWindowDataCollector(int assetId, int groupId)
            : base(assetId, groupId, eCrowdsourceType.SlidingWindow)
        {

        }

        protected override int[] Collect()
        {
            try
            {
                using (IserviceClient catalogClient = GetCatalogClient())
                {
                    string catalogSignString = Guid.NewGuid().ToString();
                    ChannelObjResponse resp = (ChannelObjResponse)catalogClient.GetResponse(new ChannelObjRequest()
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
                        m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, TVinciShared.WS_Utils.GetTcmConfigValue("CatalogSignatureKey")),
                        m_nPageIndex = 0,
                        m_nPageSize = 20
                    });
                    _channelData = resp.ChannelObj;
                    if (_channelData != null && _channelData.m_oMedias.Length > 0)
                    {
                        return resp.ChannelObj.m_oMedias;
                    }
                    else return null;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error collecting items - Exception: \n {2}", DateTime.UtcNow, CollectorType, ex.Message), "Crowdsourcing.log");
                return null;
            }
        }

        protected override Dictionary<int, BaseCrowdsourceItem> Normalize(SingularItem item)
        {
            try
            {
                Dictionary<int, BaseCrowdsourceItem> normalizedDictionary = null;
                using (WS_Catalog.IserviceClient client = GetCatalogClient())
                {
                    string catalogSignString = Guid.NewGuid().ToString();
                    if (_channelData != null && _channelData.m_OrderObject.m_bIsSlidingWindowField)
                    {

                        AssetStatsResponse assetStats = (AssetStatsResponse)client.GetResponse(new AssetStatsRequest()
                            {
                                m_nGroupID = GroupId,
                                m_type = StatsType.MEDIA,
                                m_nAssetIDs = new[] { item.Id },
                                m_dStartDate = _channelData.m_OrderObject.m_dSlidingWindowStartTimeField,
                                m_dEndDate = DateTime.UtcNow,
                                m_sSignString = catalogSignString,
                                m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, TVinciShared.WS_Utils.GetTcmConfigValue("CatalogSignatureKey")),

                            });

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
                                        Action = _channelData.m_OrderObject.m_eOrderBy,
                                        MediaId = item.Id,
                                        MediaName = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_sName,
                                        MediaImage =
                                            ((MediaObj)mediaInfo.Value.m_lObj[0]).m_lPicture.Select(
                                                pic => new BaseCrowdsourceItem.Pic()
                                                {
                                                    Size = pic.m_sSize,
                                                    URL = pic.m_sURL
                                                }).ToArray(),
                                        TimeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow),
                                        Order = item.Order,

                                    };
                                    //switch (croudsourceItem.Action)
                                    //{
                                    //    case ApiObjects.SearchObjects.OrderBy.VIEWS:
                                    //        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_nViews;
                                    //        break;
                                    //    case ApiObjects.SearchObjects.OrderBy.RATING:
                                    //        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_nVotes;
                                    //        break;
                                    //    case ApiObjects.SearchObjects.OrderBy.LIKE_COUNTER:
                                    //        croudsourceItem.ActionVal = assetStats.m_lAssetStat[0].m_nLikes;
                                    //        break;
                                    //}
                                    normalizedDictionary.Add(mediaInfo.Key.ID, croudsourceItem);
                                }

                            }
                        }

                    }

                    return normalizedDictionary;
                }
            }
            catch (Exception ex)
            {

                Logger.Logger.Log("Crowdsource", string.Format("{0}:{1} - Error normalizing singular item. mediaId {2} - Exception: \n {3}", DateTime.UtcNow, CollectorType, item.Id, ex.Message), "Crowdsourcing.log");
                return null;
            }



        }

        private static DateTime GetSlidingWindowStart(int minPeriodId)
        {
            switch (minPeriodId)
            {
                case 1:
                    return DateTime.UtcNow.AddMinutes(-1);
                case 5:
                    return DateTime.UtcNow.AddMinutes(-5);
                case 10:
                    return DateTime.UtcNow.AddMinutes(-10);
                case 30:
                    return DateTime.UtcNow.AddMinutes(-30);
                case 60:
                    return DateTime.UtcNow.AddHours(-1);
                case 120:
                    return DateTime.UtcNow.AddHours(-2);
                case 180:
                    return DateTime.UtcNow.AddHours(-3);
                case 360:
                    return DateTime.UtcNow.AddHours(-6);
                case 540:
                    return DateTime.UtcNow.AddHours(-9);
                case 720:
                    return DateTime.UtcNow.AddHours(-12);
                case 1080:
                    return DateTime.UtcNow.AddHours(-18);
                case 1440:
                    return DateTime.UtcNow.AddDays(-1);
                case 2880:
                    return DateTime.UtcNow.AddDays(-2);
                case 4320:
                    return DateTime.UtcNow.AddDays(-3);
                case 7200:
                    return DateTime.UtcNow.AddDays(-5);
                case 10080:
                    return DateTime.UtcNow.AddDays(-7);
                case 20160:
                    return DateTime.UtcNow.AddDays(-14);
                case 30240:
                    return DateTime.UtcNow.AddDays(-21);
                case 40320:
                    return DateTime.UtcNow.AddDays(-28);
                case 40321:
                    return DateTime.UtcNow.AddDays(-28);
                case 43200:
                    return DateTime.UtcNow.AddDays(-30);
                case 44600:
                    return DateTime.UtcNow.AddDays(-31);
                case 1111111:
                    return DateTime.UtcNow.AddMonths(-1);
                case 2222222:
                    return DateTime.UtcNow.AddMonths(-2);
                case 3333333:
                    return DateTime.UtcNow.AddMonths(-3);
                case 4444444:
                    return DateTime.UtcNow.AddMonths(-4);
                case 5555555:
                    return DateTime.UtcNow.AddMonths(-5);
                case 6666666:
                    return DateTime.UtcNow.AddMonths(-6);
                case 9999999:
                    return DateTime.UtcNow.AddMonths(-7);
                case 11111111:
                    return DateTime.UtcNow.AddYears(-1);
                case 22222222:
                    return DateTime.UtcNow.AddYears(-2);
                case 33333333:
                    return DateTime.UtcNow.AddYears(-3);
                case 44444444:
                    return DateTime.UtcNow.AddYears(-4);
                case 55555555:
                    return DateTime.UtcNow.AddYears(-5);
                case 66666666:
                    return DateTime.UtcNow.AddYears(-6);
                case 77777777:
                    return DateTime.UtcNow.AddYears(-7);
                case 88888888:
                    return DateTime.UtcNow.AddYears(-8);
                case 99999999:
                    return DateTime.UtcNow.AddYears(-9);
                case 100000000:
                    return DateTime.UtcNow.AddYears(-10);

                default:
                    return DateTime.MinValue;
            }
        }
    }
}
