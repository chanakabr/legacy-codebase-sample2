using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public enum eOperation { index = 0, create = 1, update = 2, delete = 3 };

    public class ESBulkRequestObj<T>
    {
        public eOperation Operation { get; set; }
        public string index { get; set; }
        public string type { get; set; }
        public T docID { get; set; }
        public string document { get; set; }
        public string routing { get; set; }

        public string ttl { get; set; }

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

    }
}
