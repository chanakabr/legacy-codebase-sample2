using Newtonsoft.Json;

namespace CouchbaseManager.Models
{
    internal class Headers
    {
        public const string CompressionPropertyName = "$$compression";
        
        [JsonProperty(CompressionPropertyName)]
        public Compression.Compression Compression { get; set; }
    }
}