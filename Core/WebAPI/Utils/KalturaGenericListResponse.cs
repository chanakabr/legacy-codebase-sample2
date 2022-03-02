using System;
using System.Collections.Generic;
using WebAPI.Models.General;

namespace WebAPI.Utils
{
    /// <summary>
    /// Generic response list, internal usage only!
    /// </summary>
    [Serializable]
    public partial class KalturaGenericListResponse<KalturaT> where KalturaT : KalturaOTTObject
    {
        /// <summary>
        /// Total items
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// A list of objects
        /// </summary>
        public List<KalturaT> Objects { get; set; }
    }
}