using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.Implementations;
using ConfigurationManager;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.WS_Catalog;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrowdsourcingFeeder.DataCollector.Implementations
{
    public class RealTimeViewsDataCollector : BaseDataCollector
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private ChannelViewsResult[] _viewsResult;

        public RealTimeViewsDataCollector(int groupId)
            : base(0, groupId, eCrowdsourceType.LiveViews)
        {

        }

        protected override int[] Collect()
        {
            try
            {
                string catalogSignString = Guid.NewGuid().ToString();
                ChannelViewsResponse response = (ChannelViewsResponse)CatalogClient.GetResponse(new ChannelViewsRequest()
                {
                    m_nGroupID = GroupId,
                    m_nPageSize = TVinciShared.WS_Utils.GetTcmIntValue("crowdsourcer.CATALOG_PAGE_SIZE"),
                    m_nPageIndex = 0,
                    m_sSignString = catalogSignString,
                    m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, ApplicationConfiguration.CatalogSignatureKey.Value),
                    m_oFilter = new Filter()
                    {

                    },
                });
                if (response != null)
                {
                    _viewsResult = response.ChannelViews;
                    return response.ChannelViews.Select(v => v.ChannelId).ToArray();
                }
                return null;

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
                Dictionary<int, BaseCrowdsourceItem> retDictionary = null;
                if (item != null)
                {
                    ChannelViewsResult channelViewsResult = _viewsResult.SingleOrDefault(x => x.ChannelId == item.Id);
                    retDictionary = new Dictionary<int, BaseCrowdsourceItem>();

                    Dictionary<LanguageObj, MediaResponse> mediaInfoDict = GetLangAndInfo(GroupId, item.Id);
                    long epochDateTime = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                    string catalogSignString = Guid.NewGuid().ToString();
                    foreach (KeyValuePair<LanguageObj, MediaResponse> mediaInfo in mediaInfoDict)
                    {
                        if (mediaInfo.Value != null && mediaInfo.Value.m_lObj.Length > 0)
                        {
                            int epgId;
                            int.TryParse(((MediaObj)mediaInfo.Value.m_lObj[0]).m_ExternalIDs, out epgId);

                            //TODO: NOT TESTED YET
                            //Set EPG ID instead of media ID
                            SelectedItem.Id = epgId != 0 ? epgId : SelectedItem.Id;

                            RealTimeViewsItem crowdsourceItem = new RealTimeViewsItem()
                            {
                                MediaId = item.Id,
                                Order = item.Order,
                                TimeStamp = epochDateTime
                            };

                            if (mediaInfo.Value != null && mediaInfo.Value.m_lObj[0] != null && channelViewsResult != null)
                            {

                                crowdsourceItem.Views = channelViewsResult.NumOfViews;
                                crowdsourceItem.MediaName = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_sName;
                                crowdsourceItem.MediaImage = ((MediaObj)mediaInfo.Value.m_lObj[0]).m_lPicture.Select(pic => new BaseCrowdsourceItem.Pic()
                                {
                                    Size = pic.m_sSize,
                                    URL = pic.m_sURL
                                }).ToArray();
                            };

                            //Get EPG item language specific info 
                            EpgResponse programInfoForLanguage = (EpgResponse)CatalogClient.GetResponse(new EpgRequest()
                            {
                                m_nGroupID = GroupId,
                                m_eSearchType = EpgSearchType.ByDate,
                                m_nNextTop = 1,
                                m_nPrevTop = 0,
                                m_nChannelIDs = new[] { epgId },
                                m_dStartDate = DateTime.UtcNow,
                                m_dEndDate = DateTime.UtcNow,
                                m_oFilter = new Filter()
                                {
                                    m_nLanguage = mediaInfo.Key.ID,
                                    m_bOnlyActiveMedia = true
                                },
                                m_sSignString = catalogSignString,
                                m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, ApplicationConfiguration.CatalogSignatureKey.Value),
                            });

                            if (programInfoForLanguage != null && programInfoForLanguage.programsPerChannel != null && programInfoForLanguage.programsPerChannel.Length > 0 && mediaInfo.Value != null && mediaInfo.Value.m_lObj[0] != null)
                            {
                                crowdsourceItem.ProgramId = programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].EPG_ID;
                                crowdsourceItem.ProgramImage = programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].PIC_URL;
                                crowdsourceItem.ProgramName = programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].NAME;
                                crowdsourceItem.EpgStartTime = TVinciShared.DateUtils.DateTimeToUnixTimestamp(TVinciShared.DateUtils.GetDateFromStr(programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].START_DATE));
                            }
                            retDictionary.Add(mediaInfo.Key.ID, crowdsourceItem);
                        }
                    }
                    log.Debug("Crowdsource - " + string.Format("Collector: {0} - Error normalizing singular item {1} - no media info: ", CollectorType, item.Id));
                }

                log.Debug("Crowdsource - " + string.Format("Collector: {0} - Error normalizing singular item is null - no EpgId: ", CollectorType));

                return retDictionary;
            }
            catch (Exception ex)
            {
                log.Error("Crowdsource - " + string.Format("Collector: {0} - Error normalizing singular item - Exception: \n {1}", CollectorType, ex.Message), ex);
                return null;
            }
        }
    }
}
