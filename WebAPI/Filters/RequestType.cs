using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Filters
{
    public enum RequestType
    {
        READ = 1,
        INSERT = 2,
        UPDATE = 4,
        WRITE = 6,
        ALL = 7
    }
}