using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Managers.Schema
{
    public enum SchemaValidationType
    {
        NULLABLE,
        FILTER_SUFFIX,
        ACTION_NAME,
        ACTION_ARGUMENTS,
        ACTION_RETURN_TYPE
    }
}