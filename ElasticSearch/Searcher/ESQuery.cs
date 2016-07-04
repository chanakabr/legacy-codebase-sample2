using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Searcher
{
    public class ESQuery
    {
        public IESTerm Query;

        public ESQuery(IESTerm query)
        {
            this.Query = query;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.AppendFormat(" \"query\": {0}", Query.ToString());
            builder.Append("}");

            string result = builder.ToString();

            return result;
        }
    }
}
