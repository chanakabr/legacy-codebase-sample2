using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader
{    
    public interface ISupportPaging
    {                
        bool TryGetItemsCount(out long count);
        int PageIndex { get; set; }
        int PageSize { get; set; }
    }
}
