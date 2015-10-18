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
        media = 0,
        channel = 1,        
        epg_internal = 2,
        epg_external = 3,
        external_channel = 4
    }
}
