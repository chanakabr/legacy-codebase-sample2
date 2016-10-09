using System.Collections.Generic;

namespace WebAPI.Models.DMS
{
    public class DMSDeviceListResponse
    {
        public DMSStatusResponse Result { get; set; }

        public List<DMSDeviceMapping> DeviceMappingList { get; set; }
    }
}