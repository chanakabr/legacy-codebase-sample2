
namespace WebAPI.Models.DMS
{
    public class DMSGroupConfigurationResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSGroupConfiguration GroupConfiguration { get; set; }

        public DMSGroupConfigurationResponse()
        {
            this.Result = new DMSStatusResponse();
        }
    }
}