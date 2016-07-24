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

        public int Size = -1;
        public int From = -1;

        public ESQuery(IESTerm query)
        {
            this.Query = query;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{");
            builder.AppendFormat(" \"query\": {0}", Query.ToString());

            if (Size > -1)
            {
                builder.AppendFormat(", \"size\": {0}", Size.ToString());
            }

            if (From > -1)
            {
                builder.AppendFormat(", \"from\": {0}", From.ToString());
            }

            builder.Append("}");

            string result = builder.ToString();

            return result;
        }
    }
}
