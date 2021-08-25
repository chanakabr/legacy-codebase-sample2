namespace ElasticSearch.Common
{
    public class NestEsBulkRequest<T>
    {
        public eOperation Operation { get; set; }
        public string Index { get; set; }
        public string DocID { get; set; }
        public T Document { get; set; }
        public string Routing { get; set; }

        public string Error { get; internal set; }

        public NestEsBulkRequest()
        {
            Operation = eOperation.index;
            Index = string.Empty;
            DocID = string.Empty;
            Document = default;
            Routing = string.Empty;
        }



        public NestEsBulkRequest(string docId, string index, T document)
        {
            DocID = docId;
            Index = index;
            Document = document;
            Operation = eOperation.index;
            Routing = string.Empty;
        }

        public NestEsBulkRequest(int docId, string index, T document) : this(docId.ToString(), index, document) { }
    }
}