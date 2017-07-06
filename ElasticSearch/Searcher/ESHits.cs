using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Searcher
{
    public class ESHits
    {
        public int total;
        public int? max_score;

        public List<ElasticSearch.Common.ElasticSearchApi.ESAssetDocument> hits;
    }
}
