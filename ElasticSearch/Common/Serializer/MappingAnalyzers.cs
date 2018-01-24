using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearch.Common
{
    public class MappingAnalyzers
    {
        public string normalIndexAnalyzer;
        public string normalSearchAnalyzer;
        public string autocompleteIndexAnalyzer;
        public string autocompleteSearchAnalyzer;
        public string suffix;
        public string phoneticIndexAnalyzer;
        public string phoneticSearchAnalyzer;
    }
}
