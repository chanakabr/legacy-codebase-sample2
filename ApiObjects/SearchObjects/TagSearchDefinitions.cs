using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    public class TagSearchDefinitions
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string Tag { get; set; }

        public string SearchValue { get; set; }

        public int TopicId { get; set; }

    }
}
