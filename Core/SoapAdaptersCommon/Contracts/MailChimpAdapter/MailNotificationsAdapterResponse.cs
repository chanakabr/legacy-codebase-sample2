using AdapaterCommon.Models;

namespace MailChimpAdapter
{
    public class MailChimpAdapterResponse
    {
        public string Data { get; set; }

        public string ProviderResponse { get; set; }

        public AdapterStatus Status { get; set; }
    }
}