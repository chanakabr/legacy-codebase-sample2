using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    public class TagSearchDefinitions
    {
        public int GroupId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string ExactSearchValue { get; set; }

        public string AutocompleteSearchValue { get; set; }

        public int TopicId { get; set; }

        public LanguageObj Language { get; set; }
    }
}
