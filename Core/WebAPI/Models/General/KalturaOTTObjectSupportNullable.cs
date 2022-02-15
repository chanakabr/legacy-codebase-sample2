using System.Collections.Generic;

namespace WebAPI.Models.General
{
    public partial class KalturaOTTObjectSupportNullable : KalturaOTTObject
    {
        internal HashSet<string> NullableProperties { get; private set; }

        public void AddNullableProperty(string str)
        {
            if (NullableProperties == null)
                NullableProperties = new HashSet<string>();

            NullableProperties.Add(str.ToLower());
        }
    }
}
