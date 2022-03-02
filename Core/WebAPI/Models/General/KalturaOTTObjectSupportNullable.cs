using System.Collections.Generic;

namespace WebAPI.Models.General
{
    public partial class KalturaOTTObjectSupportNullable : KalturaOTTObject
    {
        internal HashSet<string> NullableProperties { get; set; }
    }
}
