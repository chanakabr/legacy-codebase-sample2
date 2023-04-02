using System.Collections.Generic;
using WebAPI.Models.Catalog.Lineup;

namespace WebAPI.Validation
{
    public interface ILineupRequestValidator
    {
        int MinPageIndex { get; }
        int DefaultPageSize { get; }
        IEnumerable<int> AllowedPageSizes { get; }
        bool ValidatePageIndex(int pageIndex);
        bool ValidatePageSize(int pageSize);
        void ValidateRequestFilter(KalturaLineupRegionalChannelFilter filter);
    }
}