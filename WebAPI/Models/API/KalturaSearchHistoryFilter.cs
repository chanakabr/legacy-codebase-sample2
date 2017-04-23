using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;


namespace WebAPI.Models.API
{
    public enum KalturaSearchHistoryOrderBy
    {
        NONE
    }

    public class KalturaSearchHistoryFilter : KalturaFilter<KalturaSearchHistoryOrderBy>
    {
        public override KalturaSearchHistoryOrderBy GetDefaultOrderByValue()
        {
            return KalturaSearchHistoryOrderBy.NONE;
        }
    }
}