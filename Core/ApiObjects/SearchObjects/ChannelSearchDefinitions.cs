using System.Collections.Generic;

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
        public bool isAllowedToViewInactiveAssets { get; set; }
        public List<long> AssetUserRuleIds { get; set; }
    }

    public enum ChannelOrderBy
    {
        Name,
        CreateDate,
        Id,
        UpdateDate
    }
}
