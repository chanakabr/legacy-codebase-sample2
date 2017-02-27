using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.SearchObjects;
using System.ServiceModel;
using Catalog.Searchers;
using ApiObjects;
using Catalog.Response;
using KLogMonitor;
using System.Reflection;
using GroupsCacheManager;

namespace Catalog
{

    public class LuceneWrapper : ISearcher
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected readonly string m_EndPointAddress;
        public LuceneWrapper()
        {
            m_EndPointAddress = Utils.GetWSURL("LUCENE_WCF");
        }

        public SearchResultsObj SearchMedias(int nGroupID, MediaSearchObj oSearch, int nLangID, bool bUseStartDate, int nIndex)
        {                                    
            SearchResultsObj oRes = null;

            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    //oRes = searcher.SearchMedias(nGroupID, oSearch, nLangID, bUseStartDate);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return oRes;
        }

        public List<string> GetAutoCompleteList(int nGroupID, MediaSearchObj oSearch, int nLangID, ref int nTotalItems)
        {
            List<string> lRes = null;

            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    lRes = searcher.GetAutoCompleteList(nGroupID, oSearch, nLangID, ref nTotalItems);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return lRes;
        }

        public SearchResultsObj GetChannelsMedias(int nGroupID, int[] channels, string mediaTypes, int nUserTypeID, ApiObjects.SearchObjects.OrderObj oOrder, int pageIndex, int pageSize)
        {
            SearchResultsObj oRes = null;

            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    oRes = searcher.GetChannelsMedias(nGroupID, channels, mediaTypes, nUserTypeID, oOrder, pageIndex, pageSize);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return oRes;
        }

        public bool UpdateRecord(int nGroupID, int nMediaID)
        {
            bool bRes = false;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    bRes = searcher.UpdateRecord(nGroupID, nMediaID);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return bRes;
        }

        public bool UpdateChannel(int nGroupID, int nChannelID)
        {
            bool bRes = false;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    bRes = searcher.UpdateChannel(nGroupID, nChannelID);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return bRes;
        }

        public bool RemoveRecord(int nGroupID, int nMediaID)
        {
            bool bRes = false;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    bRes = searcher.RemoveRecord(nGroupID, nMediaID);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return bRes;
        }

        public bool ClearAndBuildGroup(int nGroupID)
        {
            bool bRes = false;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);
            try
            {
                if (searcher != null)
                {
                    bRes = searcher.ClearAndBuildGroup(nGroupID);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return bRes;
        }

        public SearchResultsObj SearchSubscriptionMedias(int nSubscriptionGroupId, List<MediaSearchObj> oSearch, int nLangID, bool bUseStartDate, string sMediaTypes, ApiObjects.SearchObjects.OrderObj oOrderObj, int nPageIndex, int nPageSize)
        {
            SearchResultsObj oRes = null;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);

            try
            {
                if (searcher != null)
                {
                    oRes = searcher.SearchSubscriptionMedias(nSubscriptionGroupId, oSearch, nLangID, bUseStartDate, sMediaTypes, oOrderObj, nPageIndex, nPageSize);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return oRes;
        }

        public bool DoesMediaBelongToChannels(int nSubscriptionGroupId, List<int> lChannelIDs, int nMediaID)
        {
            throw new NotImplementedException("DoesMediaBelongToChannels");
        }

        public List<ChannelContainObj> GetSubscriptionContainingMedia(List<ChannelContainSearchObj> oSearch)
        {
            List<ChannelContainObj> oRes = null;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);

            try
            {
                if (searcher != null)
                {
                    oRes = searcher.GetSubscriptionContainingMedia(oSearch);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return oRes;
        }
        //
        public SearchResult GetDoc(int nGroupID, int nMediaID)
        {
            throw new NotImplementedException("GetDoc");
        }

        public List<int> GetMediaChannels(int nGroupID, int nMediaID)
        {
            throw new NotImplementedException("GetMediaChannels");

        }

        public List<List<string>> GetChannelsDefinitions(List<List<long>> listsOfChannelIDs, long groupID)
        {
            throw new NotImplementedException("GetChannelsDefinitions");
        }

        public Dictionary<long, bool> ValidateMediaIDsInChannels(int nGroupID, List<long> distinctMediaIDs,
            List<string> jsonizedChannelsDefinitionsMediasHaveToAppearInAtLeastOne,
            List<string> jsonizedChannelsDefinitionsMediasMustNotAppearInAll)
        {
            throw new NotImplementedException("ValidateMediaIDsInChannels");
        }

        #region EPG
        public SearchResultsObj SearchEpgs(EpgSearchObj epgSearch)
        {
            SearchResultsObj oRes = null;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);

            try
            {
                if (searcher != null)
                {
                    oRes = searcher.SearchEpgs(epgSearch);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return oRes;
        }

        public List<string> GetEpgAutoCompleteList(EpgSearchObj epgSearch)
        {
            List<string> oRes = null;
            ISearcher searcher = Searchers.Helper.GetFactoryChannel<ISearcher>(m_EndPointAddress);

            try
            {
                if (searcher != null)
                {
                    oRes = searcher.GetEpgAutoCompleteList(epgSearch);
                }
            }
            catch { }
            finally
            {
                if (searcher != null)
                    Searchers.Helper.CloseChannel(searcher as IClientChannel);
            }
            return oRes;
        }      

        #endregion

        #region ISearcher Methods

        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to)
        {
            throw new NotImplementedException();
        }

        public List<UnifiedSearchResult> FillUpdateDates(int groupId, List<UnifiedSearchResult> assets, ref int totalItems, int pageSize, int pageIndex)
        {
            throw new NotImplementedException();
        }

        public List<UnifiedSearchResult> MultipleUnifiedSearch(int groupId, List<UnifiedSearchDefinitions> unifiedSearchDefinitions, ref int totalItems)
        {
            throw new NotImplementedException();
        }
        public List<UnifiedSearchResult> SearchSubscriptionAssets(int subscriptionGroupId, List<BaseSearchObject> searchObjects, int languageId, bool useStartDate,
            string mediaTypes, ApiObjects.SearchObjects.OrderObj order, int pageIndex, int pageSize, ref int totalItems)
        {
            throw new NotImplementedException();
        } 

        public List<int> GetEntitledEpgLinearChannels(Group group, UnifiedSearchDefinitions definitions)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ISearcher Members


        public List<UnifiedSearchResult> UnifiedSearch(UnifiedSearchDefinitions unifiedSearch, ref int totalItems, ref int to, out Dictionary<string, Dictionary<string, int>> aggregationResult)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
