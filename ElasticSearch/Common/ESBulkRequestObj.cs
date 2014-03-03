using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ElasticSearch.Common
{
    public class ESBulkRequestObj<T>
    {
        public string index { get; set; }
        public string type { get; set; }
        public T docID { get; set; }
        public string document { get; set; }
        public string routing { get; set; }

    }
}
