using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.General
{
    /// <summary>
    /// Base filter
    /// </summary>
    public abstract class KalturaFilter : KalturaOTTObject
    {
        public abstract object GetDefaultOrderByValue();
    }
}