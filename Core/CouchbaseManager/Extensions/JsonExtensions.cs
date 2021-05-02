using Newtonsoft.Json.Linq;

namespace CouchbaseManager.Extensions
{
    internal static class JsonExtensions
    {
        public static bool IsObject(this JToken token)
        {
            return token.Type == JTokenType.Object;
        }
    }
}