using ApiObjects;
using ApiObjects.Base;
using ApiObjects.SearchObjects;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
using GroupsCacheManager;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace Core.Catalog.Request
{
    /**************************************************************************
   * Get Channel + It's Medias List
   * return :
   * Channel with it's media List
   * ************************************************************************/
    [Serializable]
    [DataContract]
    public class ChannelRequest : BaseRequest, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public Int32 m_nChannelID;
        [DataMember]
        public bool m_bIgnoreDeviceRuleID;
        [DataMember]
        public ApiObjects.SearchObjects.OrderObj m_oOrderObj;

        public ChannelRequest()
            : base()
        {
            m_bIgnoreDeviceRuleID = false;
        }

        public ChannelRequest(Int32 nChannelID, Int32 nGroupID, Int32 nPageSize, Int32 nPageIndex, string sUserIP, Filter oFilter, string sSignature, string sSignString, ApiObjects.SearchObjects.OrderObj oOrderObj)
            : base(nPageSize, nPageIndex, sUserIP, nGroupID, oFilter, sSignature, sSignString)
        {
            m_nChannelID = nChannelID;
            m_oOrderObj = oOrderObj;
            m_bIgnoreDeviceRuleID = false;
        }

        public ChannelRequest(ChannelRequest c)
            : base(c.m_nPageSize, c.m_nPageIndex, c.m_sUserIP, c.m_nGroupID, c.m_oFilter, c.m_sSignature, c.m_sSignString)
        {
            m_nChannelID = c.m_nChannelID;
            m_oOrderObj = c.m_oOrderObj;
            m_bIgnoreDeviceRuleID = c.m_bIgnoreDeviceRuleID;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            ChannelResponse response = new ChannelResponse();

            try
            {
                ChannelRequest request = oBaseRequest as ChannelRequest;
                if (request == null || request.m_nChannelID == 0)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                log.DebugFormat("starting ChannelRequest.GetResponse, ChannelID:{0}", m_nChannelID);

                bool isGroupAndChannelValid = false, doesGroupUsesTemplates = false;
                int groupId = 0;
                LanguageObj defaultLanguage = null;
                List<string> permittedWatchRules = null;
                GroupsCacheManager.Channel channel = null;

                try
                {
                    if (CatalogManager.Instance.DoesGroupUsesTemplates(request.m_nGroupID))
                    {
                        doesGroupUsesTemplates = true;
                        groupId = request.m_nGroupID;
                        CatalogGroupCache catalogGroupCache;
                        if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                        {
                            log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling ChannelRequest.GetResponse", groupId);
                            return response;
                        }
                        defaultLanguage = catalogGroupCache.GetDefaultLanguage();

                        var contextData = new ContextData(groupId) { UserId = 0 };
                        var channelResponse = ChannelManager.Instance.GetChannelById(contextData, request.m_nChannelID, false);
                        if (!channelResponse.HasObject())
                        {
                            return response;
                        }

                        channel = channelResponse.Object;
                        isGroupAndChannelValid = true;
                    }
                    else
                    {
                        var groupManager = new GroupManager();
                        CatalogCache catalogCache = CatalogCache.Instance();
                        int nParentGroupID = catalogCache.GetParentGroup(request.m_nGroupID);
                        Group group = null;
                        groupManager.GetGroupAndChannel(request.m_nChannelID, nParentGroupID, ref group, ref channel);
                        isGroupAndChannelValid = group != null && channel != null;
                        if (isGroupAndChannelValid)
                        {
                            groupId = group.m_nParentGroupID;
                            defaultLanguage = group.GetGroupDefaultLanguage();
                            permittedWatchRules = group.m_sPermittedWatchRules;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("ChannelRequest - " + string.Format("failed to get GetGroupAndChannel channelID={0}, ex={1} , st: {2}", request.m_nChannelID, ex.Message, ex.StackTrace), ex);
                }

                if (isGroupAndChannelValid)
                {
                    if (channel.m_nChannelTypeID == (int)ChannelType.KSQL)
                    {
                        if (!doesGroupUsesTemplates)
                        {
                            response.m_nTotalItems = 0;
                            response.m_nMedias = null;
                            return response;
                        }

                        response = GetResponseForKSQLChannel(groupId, channel, request, defaultLanguage);
                    }
                    else
                    {
                        response = GetResponseForNonKSQLChannel(groupId, channel, request, defaultLanguage, permittedWatchRules);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                log.Error("ES Error - " + String.Concat("Exception. Req: ", ToString(), " Msg: ", ex.Message, " Type: ", ex.GetType().Name, " ST: ", ex.StackTrace), ex);
                throw ex;
            }
        }

        public static bool IsSlidingWindow(GroupsCacheManager.Channel channel)
        {
            bool bResult = false;

            if (channel.m_OrderObject.m_bIsSlidingWindowField)
            {
                bResult = typeof(OrderBy).GetMember(channel.m_OrderObject.m_eOrderBy.ToString())[0].GetCustomAttributes(typeof(SlidingWindowSupportedAttribute), false).Length > 0;
            }

            return bResult;
        }

        public static void OrderMediasByOrderNum(ref List<int> medias, GroupsCacheManager.Channel channel, ApiObjects.SearchObjects.OrderObj oOrderObj)
        {
            if (oOrderObj.m_eOrderBy.Equals(OrderBy.ID))
            {
                IEnumerable<int> ids;

                if (channel.ManualAssets?.Count > 0)
                {
                    if (oOrderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                    {
                        medias = channel.ManualAssets.OrderByDescending(x => x.OrderNum).Select(x => (int)x.AssetId).ToList();
                    }
                    else
                    {
                        medias = channel.ManualAssets.OrderBy(x => x.OrderNum).Select(x => (int)x.AssetId).ToList();
                    }

                    return;
                }

                if (channel.m_lManualMedias == null)
                {
                    channel.m_lManualMedias = new List<GroupsCacheManager.ManualMedia>();
                }

                if (oOrderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                {
                    ids = from m in medias
                          join manual in channel.m_lManualMedias
                          on m.ToString() equals manual.m_sMediaId
                          orderby manual.m_nOrderNum descending
                          select int.Parse(manual.m_sMediaId);
                }
                else
                {
                    ids = from m in medias
                          join manual in channel.m_lManualMedias
                          on m.ToString() equals manual.m_sMediaId
                          orderby manual.m_nOrderNum
                          select int.Parse(manual.m_sMediaId);
                }

                medias = ids.ToList();
            }
        }

        protected virtual MediaSearchObj GetManualSearchObject(GroupsCacheManager.Channel channel, ChannelRequest request, int nParentGroupID, LanguageObj oLanguage, List<string> lPermittedWatchRules)
        {
            int[] nDeviceRuleId = null;
            if (request.m_oFilter != null)
                nDeviceRuleId = Api.api.GetDeviceAllowedRuleIDs(request.m_nGroupID, request.m_oFilter.m_sDeviceId, request.domainId).ToArray();

            return CatalogLogic.BuildBaseChannelSearchObject(channel, request, request.m_oOrderObj, nParentGroupID, channel.m_nGroupID == channel.m_nParentGroupID ? lPermittedWatchRules : null, nDeviceRuleId, oLanguage);
        }

        private ChannelResponse GetResponseForNonKSQLChannel(int groupId, GroupsCacheManager.Channel channel, ChannelRequest request, LanguageObj defaultLanguage, List<string> permittedWatchRules)
        {
            var response = new ChannelResponse();
            MediaSearchObj channelSearchObject = GetManualSearchObject(channel, request, groupId, defaultLanguage, permittedWatchRules);
            channelSearchObject.m_bIgnoreDeviceRuleId = request.m_bIgnoreDeviceRuleID;

            int nPageIndex = 0;
            int nPageSize = 0;

            if (channel.m_nChannelTypeID == (int)ChannelType.Manual || IsSlidingWindow(channel))
            {
                nPageIndex = channelSearchObject.m_nPageIndex;
                nPageSize = channelSearchObject.m_nPageSize;
                channelSearchObject.m_nPageSize = 0;
                channelSearchObject.m_nPageIndex = 0;
            }

            IIndexManager indexManager = IndexManagerFactory.Instance.GetIndexManager(m_nGroupID);

            SearchResultsObj oSearchResults = indexManager.SearchMedias(channelSearchObject, request.m_oFilter.m_nLanguage, request.m_oFilter.m_bUseStartDate);
            if (oSearchResults == null || oSearchResults.m_resultIDs == null || oSearchResults.m_resultIDs.Count == 0)
            {
                return response;
            }

            List<int> medias = oSearchResults.m_resultIDs.Select(item => item.assetID).ToList();
            int nTotalItems = oSearchResults.n_TotalItems;
            List<SearchResult> lMediaRes =
                oSearchResults.m_resultIDs.Select(
                    item => new SearchResult() { assetID = item.assetID, UpdateDate = item.UpdateDate }).ToList();

            if (IsSlidingWindow(channel))
            {
                medias = indexManager.OrderMediaBySlidingWindow(channel.m_OrderObject.m_eOrderBy, channel.m_OrderObject.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC, nPageSize, nPageIndex, medias, channel.m_OrderObject.m_dSlidingWindowStartTimeField);
                nTotalItems = 0;

                if (medias != null && medias.Count > 0)
                {
                    nTotalItems = medias.Count;
                    Dictionary<int, SearchResult> dMediaRes = lMediaRes.ToDictionary(item => item.assetID);
                    lMediaRes.Clear();
                    foreach (int item in medias)
                    {
                        if (dMediaRes.ContainsKey(item))
                            lMediaRes.Add(dMediaRes[item]);
                    }
                }
                else
                {
                    lMediaRes.Clear();
                }
            }

            if (channel.m_nChannelTypeID == (int)ChannelType.Manual)
            {
                OrderMediasByOrderNum(ref medias, channel, channelSearchObject.m_oOrder);

                int nValidNumberOfMediasRange = nPageSize;
                if (Utils.ValidatePageSizeAndPageIndexAgainstNumberOfMedias(medias.Count, nPageIndex, ref nValidNumberOfMediasRange))
                {
                    if (nValidNumberOfMediasRange > 0)
                    {
                        medias = medias.GetRange(nPageSize * nPageIndex, nValidNumberOfMediasRange);
                    }
                }
                else
                {
                    medias.Clear();
                }

                if (medias != null && medias.Count > 0)
                {
                    Dictionary<int, SearchResult> dMediaRes = lMediaRes.ToDictionary(item => item.assetID);
                    lMediaRes = new List<SearchResult>();
                    foreach (int item in medias)
                    {
                        if (dMediaRes.ContainsKey(item))
                        {
                            lMediaRes.Add(dMediaRes[item]);
                        }
                    }
                }
            }

            if (medias == null || medias.Count == 0)
            {
                response.m_nMedias = null;
                response.m_nTotalItems = 0;
                return response;
            }

            response.m_nTotalItems = nTotalItems;

            if (lMediaRes.Count > 0)
            {
                response.m_nMedias = new List<SearchResult>(lMediaRes);
            }
            else
            {
                response.m_nMedias = null;
            }

            return response;
        }

        private ChannelResponse GetResponseForKSQLChannel(int groupId, GroupsCacheManager.Channel channel, ChannelRequest request, LanguageObj defaultLanguage)
        {
            var response = new ChannelResponse();

            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("(and asset_type='media' ");
            var requestMultiFiltering = request as ChannelRequestMultiFiltering;

            if (requestMultiFiltering != null &&
                requestMultiFiltering.m_lFilterTags != null &&
                requestMultiFiltering.m_lFilterTags.Count > 0 &&
                requestMultiFiltering.m_eFilterCutWith != CutWith.WCF_ONLY_DEFAULT_VALUE)
            {
                if (requestMultiFiltering.m_eFilterCutWith == CutWith.AND)
                {
                    stringBuilder.Append("(and ");
                }
                else
                {
                    stringBuilder.Append("(or ");
                }

                foreach (var filterTag in requestMultiFiltering.m_lFilterTags)
                {
                    stringBuilder.AppendFormat("{0}='{1}'", filterTag.m_sKey, filterTag.m_sValue);
                }

                stringBuilder.Append(")");
            }
            stringBuilder.Append(")");

            // build request
            var internalChannelRequest = new InternalChannelRequest()
            {
                m_sSignature = Utils.GetSignature(request.m_sSignString, request.m_nGroupID),
                m_sSignString = request.m_sSignString,
                m_oFilter = request.m_oFilter,
                m_sUserIP = request.m_sUserIP,
                m_nGroupID = groupId,
                m_nPageIndex = request.m_nPageIndex,
                m_nPageSize = request.m_nPageSize,
                m_sSiteGuid = request.m_sSiteGuid,
                domainId = domainId,
                order = request.m_oOrderObj,
                internalChannelID = channel.m_nChannelID.ToString(),
                filterQuery = stringBuilder.ToString(),
                m_dServerTime = request.m_dServerTime,
                m_bIgnoreDeviceRuleID = request.m_bIgnoreDeviceRuleID,
                isAllowedToViewInactiveAssets = false
            };

            var orderBy = request.m_oOrderObj != null ? request.m_oOrderObj.m_eOrderBy : OrderBy.NONE;
            var unifiedSearchResponse = internalChannelRequest.GetResponse(internalChannelRequest) as UnifiedSearchResponse;

            if (!unifiedSearchResponse.status.IsOkStatusCode())
            {
                log.ErrorFormat("Error in GetResponse for InternalChannelRequest, status:{0}", unifiedSearchResponse.status.ToString());
            }

            response.m_nTotalItems = unifiedSearchResponse.m_nTotalItems;
            if (unifiedSearchResponse.searchResults != null && unifiedSearchResponse.searchResults.Count > 0)
            {
                response.m_nMedias.AddRange(unifiedSearchResponse.searchResults.Select(x => new SearchResult() { assetID = int.Parse(x.AssetId), UpdateDate = x.m_dUpdateDate }));
            }
            else
            {
                response.m_nMedias = null;
            }

            return response;
        }
    }
}