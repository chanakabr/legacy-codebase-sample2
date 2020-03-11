using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Tvinci.Web.Controls.Gallery.Part
{
    public class PagingPartItem : Control, INamingContainer
    {
        public int ActivePageIndex { get; set; }
        public long PagesCount { get; set; }
        public int PageSize { get; set; }
        public long TotalItemsCount { get; set; }

        public bool IsFirstPage()
        {
            return ActivePageIndex == 0;
        }

        public bool IsLastPage()
        {
            return (ActivePageIndex == PagesCount - 1);
        }

        public long LastShownItemNumber
        {
            get
            {
                if (PageSize == 0)
                {
                    return TotalItemsCount;
                }
                else
                {
                    int temp = PageSize + FirstShownItemNumber - 1;
                    return temp > TotalItemsCount ? TotalItemsCount : temp;
                }
            }
        }

        public int FirstShownItemNumber
        {
            get
            {
                if (TotalItemsCount == 0)
                {
                    return 0;
                }
                else
                {
                    return PageSize * (ActivePageIndex) + 1;
                }
            }
        }

        public PagingPartItem(int activePageIndex, long pagesCount, long totalItemsCount, int pageSize)
        {
            TotalItemsCount = totalItemsCount;
            PageSize = pageSize;
            ActivePageIndex = activePageIndex;
            PagesCount = pagesCount;
        }
    }
}
