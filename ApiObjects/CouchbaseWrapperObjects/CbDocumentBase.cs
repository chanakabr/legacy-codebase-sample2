using Newtonsoft.Json;

namespace ApiObjects.CouchbaseWrapperObjects
{
    [JsonObject]
    public abstract class CbDocumentBase
    {
        [JsonProperty("id")]
        public abstract string Id { get; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
