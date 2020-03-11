
namespace WebAPI.Models.DMS
{
    public class DMSTagResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSTagMapping TagMap { get; set; }

        public DMSTagResponse()
        {
            this.Result = new DMSStatusResponse();
        }
    }
}