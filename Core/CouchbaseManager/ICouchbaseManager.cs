using Newtonsoft.Json;

namespace CouchbaseManager
{
    public interface ICouchbaseManager
    {
        bool Set<T>(string key, T value, uint expiration);
        
        bool SetWithVersion<T>(string key, T content, ulong version, uint expiration);
        
        T Get<T>(string key, out eResultStatus status, JsonSerializerSettings settings = null);
        
        T GetWithVersion<T>(string key, out ulong version, out eResultStatus status, JsonSerializerSettings settings = null);
    }
}