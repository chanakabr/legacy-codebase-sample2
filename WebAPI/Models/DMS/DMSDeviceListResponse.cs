using System.Collections.Generic;

namespace WebAPI.Models.DMS
{
    public class DMSDeviceListResponse
    {
        public DMSStatusResponse Result { get; set; }

        public List<DMSDeviceMapping> DeviceMapList { get; set; }

        public long TotalNumOfResults { get; set; }       
    }
}