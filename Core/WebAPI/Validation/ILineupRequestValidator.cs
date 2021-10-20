using System.Collections.Generic;

namespace WebAPI.Validation
{
    public interface ILineupRequestValidator
    {
        int MinPageIndex { get; }
        int DefaultPageSize { get; }
        IEnumerable<int> AllowedPageSizes { get; }
        bool ValidatePageIndex(int pageIndex);
        bool ValidatePageSize(int pageSize);
    }
}