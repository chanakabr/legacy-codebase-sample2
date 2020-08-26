
namespace ApiObjects.Base
{
    public class CorePager
    {
        private const int DEFAULT_PAGE_SIZE = 30;
        private const int DEFAULT_PAGE_INDEX = 1;

        public int PageSize;
        public int PageIndex;

        public CorePager()
        {
            PageSize = DEFAULT_PAGE_SIZE;
            PageIndex = DEFAULT_PAGE_INDEX;
        }
    }
}
