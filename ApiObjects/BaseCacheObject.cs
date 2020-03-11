using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public abstract class BaseCacheObject
    {
        abstract public string GetCacheKey(Int32 nObjectID);
    }
}
