using ApiObjects.SearchObjects;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Searcher
{
    public abstract class EsBaseOrderByField : IEsOrderByField
    {
        protected EsBaseOrderByField(OrderDir direction)
        {
            OrderByDirection = direction;
        }

        public virtual string EsField => null;
        public OrderDir OrderByDirection { get; }

        public JObject EsOrderByObject =>
            !string.IsNullOrEmpty(EsField)
                ? new JObject
                {
                    [EsField] = new JObject
                    {
                        ["order"] = JToken.FromObject(OrderByDirection.ToString().ToLower())
                    }
                }
                : null;
    }
}
