
namespace WebAPI.Models.DMS
{
    public class DMSDeviceResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSDeviceMapping DeviceMap { get; set; }
    }
}