using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using AdapaterCommon.Models;
using REAdapter.Models;

namespace REAdapter
{
    [ServiceContract]
    public interface IService
    {
        [OperationContract]
        AdapterStatus SetConfiguration(int recommendationEngineId,
                                       KeyValue[] settings, int partnerId, long timeStamp, string signature);

        [OperationContract]
        RecommendationsResult GetChannelRecommendations(int recommendationEngineId, string externalChannelId, KeyValue[] enrichments, string freeParam,
            long timeStamp, string signature, int pageIndex, int pageSize);

        [OperationContract]
        RecommendationsResult GetRelatedRecommendations(int recommendationEngineId, int mediaId, int mediaTypeId,
                                                               KeyValue[] enrichments, string freeParam,
                                                               int[] filterTypeIds, int pageIndex, int pageSize,
                                                               long timeStamp, string signature);

        [OperationContract]
        RecommendationsResult GetSearchRecommendations(int recommendationEngineId, string query,
                                                               KeyValue[] enrichments,
                                                               int[] filterTypeIds, int pageIndex, int pageSize,
                                                               long timeStamp, string signature);

        [OperationContract]
        void ShareFilteredResponse(int recommendationEngineId, List<SearchResult> searchResults);
    }
}
