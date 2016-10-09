
namespace WebAPI.Models.DMS
{
    public class DMSReportDeviceGetResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSDevice Device { get; set; }     
    }
}