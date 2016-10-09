using System.Collections.Generic;

namespace WebAPI.Models.DMS
{
    public class DMSReportDeviceListResponse
    {
        public DMSStatusResponse Result { get; set; }

        public List<DMSDevice> DeviceList { get; set; }       
    }
}