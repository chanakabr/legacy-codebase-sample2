using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.General
{
    enum KalturaDummyOrderBy
    {
    }

    /// <summary>
    /// Define KalturaRelatedObjectFilter
    /// </summary>
    [SchemeBase(typeof(KalturaFilter<KalturaDummyOrderBy>))]
    public interface KalturaRelatedObjectFilter : IKalturaFilter
    {
        // no implementation 
    }
}