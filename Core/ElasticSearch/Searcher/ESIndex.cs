using System;
using System.Collections.Generic;
using System.Text;

namespace ElasticSearch.Searcher
{
    public class ESIndex
    {
        public string Name { get; set; }
        public IEnumerable<string> Aliases { get; set; }
    }
}
