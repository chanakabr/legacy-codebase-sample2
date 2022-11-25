using System;
using System.Collections.Generic;

namespace ApiObjects.SearchObjects.GroupRepresentatives
{
    public class GroupRepresentativesRequest
    {
        public int PartnerId { get; set; }
        public long UserId { get; set; }
        public long DomainId { get; set; }
        public string Udid { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int LanguageId { get; set; }
        public string Filter { get; set; }
        public string GroupByValue { get; set; }
        public UnmatchedItemsPolicy UnmatchedItemsPolicy { get; set; }
        public bool IsAllowedToViewInactiveAssets { get; set; }
        public RepresentativeSelectionPolicy SelectionPolicy { get; set; }
        public bool UseStartDate { get; set; }
        public bool GetOnlyActiveAssets { get; set; }
        public string UserIp { get; set; }
        public IReadOnlyCollection<AssetOrder> OrderingParameters { get; set; }
    }
}