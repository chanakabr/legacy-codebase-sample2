using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.CrowdsourceItems;
using ApiObjects.CrowdsourceItems.Base;
using ApiObjects.CrowdsourceItems.Implementations;
using ApiObjects.SearchObjects;
using CrowdsourcingFeeder.DataCollector.Base;
using CrowdsourcingFeeder.WS_Catalog;
using Tvinci.Core.DAL;

namespace CrowdsourcingFeeder.DataCollector.Implementations
{
    public class RealTimeViewsDataCollector : BaseDataCollector
    {
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
                    m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, TVinciShared.WS_Utils.GetTcmConfigValue("crowdsourcer.CatalogSignatureKey")),
                    m_oFilter = new Filter()
                    {

                    }
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
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error collecting items - Exception: \n {2}", DateTime.UtcNow, CollectorType, ex.Message), "Crowdsourcing");
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
                    
                    string catalogSignString = Guid.NewGuid().ToString();
                    foreach (KeyValuePair<LanguageObj, MediaResponse> mediaInfo in mediaInfoDict)
                    {
                        RealTimeViewsItem crowdsourceItem = new RealTimeViewsItem()
                        {
                            MediaId = item.Id,
                            Order = item.Order
                        };
                        
                        if (mediaInfo.Value != null && mediaInfo.Value.m_lObj[0] != null)
                        {
 
                            crowdsourceItem.Views = channelViewsResult.NumOfViews;
                            crowdsourceItem.MediaName = ((MediaObj) mediaInfo.Value.m_lObj[0]).m_sName;
                            crowdsourceItem.MediaImage = ((MediaObj) mediaInfo.Value.m_lObj[0]).m_lPicture.Select(pic => new BaseCrowdsourceItem.Pic()
                            {
                                Size = pic.m_sSize,
                                URL = pic.m_sURL
                            }).ToArray();
                        };
                        
                        //Get EPG item language specific info 
                        EpgResponse programInfoForLanguage = (EpgResponse)CatalogClient.GetResponse(new EpgRequest()
                        {
                            m_nGroupID = GroupId,
                            m_eSearchType = EpgSearchType.Current,
                            m_nNextTop = 1,
                            m_nPrevTop = 0,
                            m_nChannelIDs = new[] { item.Id },
                            m_oFilter = new Filter()
                            {
                                m_nLanguage = mediaInfo.Key.ID,
                                m_bOnlyActiveMedia = true
                            },
                            m_sSignString = "",
                            m_sSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, TVinciShared.WS_Utils.GetTcmConfigValue("crowdsourcer.CatalogSignatureKey")),
                        });

                        if (programInfoForLanguage != null && mediaInfo.Value.m_lObj[0] != null)
                        {
                            crowdsourceItem.TimeStamp = TVinciShared.DateUtils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                            crowdsourceItem.ProgramId = programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].EPG_ID;
                            crowdsourceItem.ProgramImage = programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].PIC_URL;
                            crowdsourceItem.ProgramName = programInfoForLanguage.programsPerChannel[0].m_lEpgProgram[0].NAME;
                        }
                        retDictionary.Add(mediaInfo.Key.ID, crowdsourceItem);
                    }
                }

                return retDictionary;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Crowdsource", string.Format("{0}: {1} - Error normalizing singular item - Exception: \n {2}", DateTime.UtcNow, CollectorType, ex.Message), "Crowdsourcing");
                return null;
            }
        }
    }
}
