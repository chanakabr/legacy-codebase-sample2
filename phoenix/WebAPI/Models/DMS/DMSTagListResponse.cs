using System.Collections.Generic;

namespace WebAPI.Models.DMS
{
    public class DMSTagListResponse
    {
        public DMSStatusResponse Result { get; set; }

        public List<DMSTagMapping> TagMappingList { get; set; }
    }
}