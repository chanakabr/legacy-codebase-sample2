
namespace WebAPI.Models.DMS
{
    public class DMSTagGetResponse
    {
        public DMSStatusResponse Result { get; set; }

        public DMSTagMapping TagMap { get; set; }

        public DMSTagGetResponse()
        {
            this.Result = new DMSStatusResponse();
        }
    }
}