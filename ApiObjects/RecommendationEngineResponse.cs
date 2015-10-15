using ApiObjects.Response;

namespace ApiObjects
{

    public class RecommendationEngineResponse
    {
        public ApiObjects.Response.Status Status { get; set; }
        public RecommendationEngine RecommendationEngine { get; set; }

        public RecommendationEngineResponse()
        {
            Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
            RecommendationEngine = new RecommendationEngine();
        }
    }
}
