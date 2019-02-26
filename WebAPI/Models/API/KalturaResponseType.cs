using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    [KalturaIntEnum]
    public enum KalturaResponseType
    {
        JSON = 1,
        XML = 2,
        JSONP = 9,
        ASSET_XML =30,
        EXCEL = 31
    }
}