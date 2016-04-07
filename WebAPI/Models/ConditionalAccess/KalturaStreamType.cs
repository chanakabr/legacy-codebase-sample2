using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaStreamType
    {
        catchup = 0,
        start_over = 1,
        trick_play = 2,
    }
}