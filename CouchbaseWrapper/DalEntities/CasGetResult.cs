using ApiObjects.CouchbaseWrapperObjects;

namespace CouchbaseWrapper.DalEntities
{
    public class CasGetResult<T> where T : CbDocumentBase
    {
        public T Value { get; set; }
        public eOperationResult OperationResult { get; set; }
        public ulong DocVersion { get; set; }
    }
}