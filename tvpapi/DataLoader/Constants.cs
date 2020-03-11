using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tvinci.Data.DataLoader
{
    public enum ePagingLocation
    {
        Source,
        Server,
        Client
    }

    public enum eCacheMode
    {        
        Application,
        Session,
        Custom,
        Never
    }
    
    [Flags]
    public enum eCacheAction
    {
        None = 0,
        GetFromCache = 2,
        StoreInCache = 4
    }
}
