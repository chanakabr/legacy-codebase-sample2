using ApiObjects.SearchObjects.Converters;
using Newtonsoft.Json;

namespace ApiObjects.SearchObjects
{
    [JsonConverter(typeof(AssetOrderConverter))]
    public class AssetOrder
    {
        public OrderBy Field { get; set; }
        public OrderDir Direction { get; set; }
    }
}