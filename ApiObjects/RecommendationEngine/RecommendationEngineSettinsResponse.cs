using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class RecommendationEngineSettinsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public List<RecommendationEngine> RecommendationEngines { get; set; }

        public RecommendationEngineSettinsResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            RecommendationEngines = new List<RecommendationEngine>();
        }

        public RecommendationEngineSettinsResponse(ApiObjects.Response.Status status, List<RecommendationEngine> recommendationEngines)
        {
            this.Status = status;
            this.RecommendationEngines = recommendationEngines;
        }
    }
}
