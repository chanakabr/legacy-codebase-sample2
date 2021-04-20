using CouchbaseManager.Models;

namespace CouchbaseManager
{
    public interface ICompressionCouchbaseManager : ICouchbaseManager
    {
        bool Set<T>(CouchbaseRecord<T> record);
    }
}