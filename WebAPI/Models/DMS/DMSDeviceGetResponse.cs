
namespace WebAPI.Models.DMS
{
    public class ReportDeviceGetResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSDevice Device { get; set; }       
    }
}