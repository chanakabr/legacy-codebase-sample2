
namespace WebAPI.Models.DMS
{
    public class DMSConfigurationResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSAppVersion Configuration { get; set; }
     
    }
}