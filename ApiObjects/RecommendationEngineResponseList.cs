using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects
{
    public class RecommendationEnginesResponseList
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<RecommendationEngineBase> RecommendationEngines { get; set; }

        public RecommendationEnginesResponseList()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            RecommendationEngines = new List<RecommendationEngineBase>();
        }

        public RecommendationEnginesResponseList(ApiObjects.Response.Status status, List<RecommendationEngineBase> recommendationEngines)
        {
            this.Status = status;
            this.RecommendationEngines = recommendationEngines;
        }
    }

}
