using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Application token status
    /// </summary>
    [KalturaIntEnum]
    public enum KalturaAppTokenStatus
    {
        DELETED = 0,
        DISABLED = 1,	
        ACTIVE = 2,		
    }
}