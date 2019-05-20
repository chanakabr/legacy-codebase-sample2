using ApiObjects;
using System.Collections.Generic;

namespace AdapterControllers
{
    public class RecommendationResult
    {
        public string id;
        public eAssetTypes type;
        public List<KeyValuePair<string, string>> TagsExtarData;
    }
}
