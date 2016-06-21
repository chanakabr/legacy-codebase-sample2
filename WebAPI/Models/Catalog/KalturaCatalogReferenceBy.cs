using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Catalog entity reference
    /// </summary>
    public enum KalturaCatalogReferenceBy
    {
        MEDIA = 0,
        EPG_INTERNAL = 2,
        EPG_EXTERNAL = 3
    }
}
