using System.Collections.Generic;
using ApiObjects;
using ApiObjects.SearchObjects;

namespace ApiLogic.Catalog
{
    public class SearchAssetsFilter
    {
        public int GroupId { get; set; }
        public string UserId { get; set; }
        public int DomainId { get; set; }
        public string SiteGuid { get; set; }
        public string Udid { get; set; }
        public string Language { get; set; }
        public int PageIndex { get; set; }
        public int? PageSize { get; set; }
        public string Filter { get; set; }
        public List<int> AssetTypes { get; set; }
        public List<int> EpgChannelIds { get; set; }
        public bool ManagementData { get; set; }
        public List<string> GroupBy { get; set; }
        public bool IsAllowedToViewInactiveAssets { get; set; }
        public bool IgnoreEndDate { get; set; }
        public GroupingOption GroupByType { get; set; } = GroupingOption.Omit;
        public bool IsPersonalListSearch { get; set; }
        public bool UseFinal { get; set; }
        public int? TrendingDays { get; set; }
    }
}
