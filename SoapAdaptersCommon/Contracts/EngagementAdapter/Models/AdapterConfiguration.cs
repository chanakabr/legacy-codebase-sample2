
namespace EngagementAdapter.Models
{
    public class AdapterConfiguration
    {
        public int PartnerId { get; set; }

        public int AdapterId { get; set; }

        public string ProviderUrl { get; set; }

        public KeyValue[] DynamicParameters { get; set; }
    }
}