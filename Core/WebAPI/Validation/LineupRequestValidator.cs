using System.Collections.Generic;
using System.Linq;

namespace WebAPI.Validation
{
    public class LineupRequestValidator : ILineupRequestValidator
    {
        private const int MIN_PAGE_INDEX = 1;
        private const int DEFAULT_PAGE_SIZE = 500;

        public int MinPageIndex => MIN_PAGE_INDEX;
        public int DefaultPageSize => DEFAULT_PAGE_SIZE;

        public IEnumerable<int> AllowedPageSizes => new[] { 100, 200, 800, 1200, 1600 };

        public bool ValidatePageIndex(int pageIndex)
        {
            return pageIndex >= MIN_PAGE_INDEX;
        }

        public bool ValidatePageSize(int pageSize)
        {
            return AllowedPageSizes.Contains(pageSize);
        }
    }
}