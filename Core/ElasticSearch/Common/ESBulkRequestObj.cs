using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public enum eOperation { index = 0, create = 1, update = 2, delete = 3 };

    
    public class NestEsBulkRequest<T,K>
    {
        public eOperation Operation { get; set; }
        public string Index { get; set; }
        public string Type { get; set; }
        public T DocID { get; set; }
        public K Document { get; set; }
        public string Routing { get; set; }

        public string TTL { get; set; }

        public string error { get; internal set; }

        public NestEsBulkRequest()
        {
            Operation = eOperation.index;
            Index = string.Empty;
            Type = string.Empty;
            DocID = default;
            Document = default;
            Routing = string.Empty;
            TTL = string.Empty;
        }

        public NestEsBulkRequest(T docId, string index, string type, K document)
        {
            DocID = docId;
            Index = index;
            Type = type;
            Document = document;
            Operation = eOperation.index;
            Routing = string.Empty;
            TTL = string.Empty;
        }

        public NestEsBulkRequest(T docId, string index, string type, K document, eOperation operation, string routing, string ttl):
            this(docId, index, type, document)
        {
            Operation = operation;
            Routing = routing;
            TTL = ttl;
        }

    }

    
    
    public class ESBulkRequestObj<T>
    {
        public eOperation Operation { get; set; }
        public string index { get; set; }
        public string type { get; set; }
        public T docID { get; set; }
        public string document { get; set; }
        public string routing { get; set; }

        public string ttl { get; set; }

        public string error { get; internal set; }

        public ESBulkRequestObj()
        {
            Operation = eOperation.index;
            index = string.Empty;
            type = string.Empty;
            docID = default(T);
            document = string.Empty;
            routing = string.Empty;
            ttl = string.Empty;
        }

        public ESBulkRequestObj(T docId, string index, string type, string document)
        {
            this.docID = docId;
            this.index = index;
            this.type = type;
            this.document = document;
            Operation = eOperation.index;
            routing = string.Empty;
            ttl = string.Empty;
        }

        public ESBulkRequestObj(T docId, string index, string type, string document, eOperation operation, string routing, string ttl)
        {
            this.docID = docId;
            this.index = index;
            this.type = type;
            this.document = document;
            this.Operation = operation;
            this.routing = routing;
            this.ttl = ttl;
        }

    }
}
