
namespace PGAdapter.Models
{  
    public class AdapterTransaction
    {
        public string Id { get; set; }

        public string UserId { get; set; }

        public string Token { get; set; }

        public Transaction Transaction { get; set; }

        public string ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ContentId { get; set; }
    }
}
