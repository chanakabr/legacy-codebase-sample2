using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using ApiObjects;
using Core.Catalog.Response;
using EpgBL;
using KLogMonitor;
using ApiObjects.SearchObjects;
using GroupsCacheManager;

namespace Core.Catalog.Request
{
    [Serializable]
    [DataContract]
    public class EPGProgramsByProgramsIdentefierRequest : BaseEpg, IRequestImp
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [DataMember]
        public string[] pids { get; set; }
       
        public EPGProgramsByProgramsIdentefierRequest()
            : base()
        {
        }

        public EPGProgramsByProgramsIdentefierRequest(EPGProgramsByProgramsIdentefierRequest epg)
            : base(epg.eLang, epg.duration, epg.m_nPageSize, epg.m_nPageIndex, epg.m_sUserIP, epg.m_nGroupID, epg.m_oFilter, epg.m_sSignature, epg.m_sSignString)
        {
            this.pids = epg.pids;
        }

        public override BaseResponse GetResponse(BaseRequest oBaseRequest)
        {
            try
            {
                EPGProgramsByProgramsIdentefierRequest request = oBaseRequest as EPGProgramsByProgramsIdentefierRequest;

                if (request == null)
                    throw new ArgumentException("request object is null or Required variables is null");

                CheckSignature(request);

                ElasticsearchWrapper elasticSearchWrapper = new ElasticsearchWrapper();

                GroupManager groupManager = new GroupManager();
                Group group = groupManager.GetGroup(request.m_nGroupID);
                var defaultLanguage = group.GetGroupDefaultLanguage();

                List<BooleanPhraseNode> nodes = new List<BooleanPhraseNode>();

                foreach (var item in request.pids)
                {
                    nodes.Add(new BooleanLeaf("epg_identifier", item, typeof(string), ComparisonOperator.Equals));
                }

                BooleanPhrase phrase = new BooleanPhrase(nodes, eCutType.Or);

                UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions()
                {
                    groupId = request.m_nGroupID,
                    langauge = defaultLanguage,
                    filterPhrase = phrase,
                    shouldSearchEpg = true,
                    pageIndex = 0,
                    pageSize = request.pids.Count(),
                    shouldAddActive = true,
                    shouldUseStartDate = false,
                    shouldUseFinalEndDate = false,
                    shouldUseSearchEndDate = false,
                    epgDaysOffest = 365
                };

                int totalItems = 0;
                int to = 0;
                var initialSearchResult = elasticSearchWrapper.UnifiedSearch(definitions, ref totalItems, ref to);

                EpgProgramsResponse response = new EpgProgramsResponse();
                BaseEpgBL epgBL = EpgBL.Utils.GetInstance(request.m_nGroupID);

                List<EPGChannelProgrammeObject> retList = null;

                if (initialSearchResult != null && initialSearchResult.Count > 0)
                {
                    retList = epgBL.GetEpgs(initialSearchResult.Select(item => Convert.ToInt32(item.AssetId)).ToList());
                }
                else
                {
                    retList = epgBL.GetEPGPrograms(request.m_nGroupID, request.pids, request.eLang, request.duration);
                }

                if (retList != null && retList.Count > 0)
                {
                    // get all linear settings about channel + group
                    CatalogLogic.GetLinearChannelSettings(request.m_nGroupID, retList);

                    response.lEpgList = FilterResult(retList, request.m_nPageIndex, request.m_nPageSize);
                    response.m_nTotalItems = retList.Count;
                }
                return response;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                return new BaseResponse();
            }
        }

         private List<EPGChannelProgrammeObject> FilterResult(List<EPGChannelProgrammeObject> epgsToFilter, int pageIndex, int pageSize)
         {
             List<EPGChannelProgrammeObject> filteredEpgs = new List<EPGChannelProgrammeObject>();
             if (epgsToFilter != null && epgsToFilter.Count > 0)
             {
                 // in order to support old requests that are sent with pageSize=0 and pageIndex=0 and expect to get all the epg's in the response
                 if (pageSize <= 0)
                 {
                     pageSize = filteredEpgs.Count;
                 }

                 filteredEpgs = epgsToFilter.OrderBy(x => x.EPG_ID).ToList();
                 int totalResults = filteredEpgs.Count;
                 int startIndexOnList = pageIndex * pageSize;
                 int rangeToGetFromList = (startIndexOnList + pageSize) > totalResults ? (totalResults - startIndexOnList) > 0 ? (totalResults - startIndexOnList) : 0 : pageSize;
                 if (rangeToGetFromList > 0)
                 {
                     filteredEpgs = filteredEpgs.GetRange(startIndexOnList, rangeToGetFromList);
                 }
                 else
                 {
                     filteredEpgs.Clear();
                 }
             }

             return filteredEpgs;
         }

    }
}
