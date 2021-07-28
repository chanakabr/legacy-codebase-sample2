namespace ElasticSearch.Common
{
    public class NestEsBulkRequest<T,K>
    {
        public eOperation Operation { get; set; }
        public string Index { get; set; }
        public T DocID { get; set; }
        public K Document { get; set; }
        public string Routing { get; set; }
        public string TTL { get; set; }

        public string Error { get; internal set; }

        public NestEsBulkRequest()
        {
            Operation = eOperation.index;
            Index = string.Empty;
            DocID = default;
            Document = default;
            Routing = string.Empty;
            TTL = string.Empty;
        }

        public NestEsBulkRequest(T docId, string index, K document)
        {
            DocID = docId;
            Index = index;
            Document = document;
            Operation = eOperation.index;
            Routing = string.Empty;
            TTL = string.Empty;
        }

        public NestEsBulkRequest(T docId, string index, K document, eOperation operation, string routing, string ttl):
            this(docId, index, document)
        {
            Operation = operation;
            Routing = routing;
            TTL = ttl;
        }

    }
}