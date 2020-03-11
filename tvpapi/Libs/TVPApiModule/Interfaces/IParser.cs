using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApi
{
    public interface IParser
    {
        string Parse(object obj, int items, int index, int groupID, long totalItemsCount, PlatformType platform);
    }
}
