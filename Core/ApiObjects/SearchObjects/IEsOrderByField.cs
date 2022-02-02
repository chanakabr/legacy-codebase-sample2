using Newtonsoft.Json.Linq;

namespace ApiObjects.SearchObjects
{
    public interface IEsOrderByField
    {
        JObject EsOrderByObject { get; }
        string EsField { get; }
        OrderDir OrderByDirection { get; }
    }
}
