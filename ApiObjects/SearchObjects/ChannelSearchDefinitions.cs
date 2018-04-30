using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.SearchObjects
{
    public class ChannelSearchDefinitions
    {
        public int GroupId { get; set; }    
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string ExactSearchValue { get; set; }
        public string AutocompleteSearchValue { get; set; }
        public List<int> SpecificChannelIds { get; set; }
        public ChannelOrderBy OrderBy { get; set; }
        public OrderDir OrderDirection { get; set; }
        public bool IsOperatorSearch { get; set; }
    }

    public enum ChannelOrderBy
    {
        Name,
        CreateDate,
        Id
    }
}
