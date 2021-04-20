using Newtonsoft.Json;

namespace CouchbaseManager.Models
{
    internal class InternalCouchbaseRecord<T>
    {
        public const string HeadersPropertyName = "$$headers";
        
        [JsonProperty(HeadersPropertyName)]
        public Headers Headers { get; set; }
        
        public T Content { get; set; }
    }
}