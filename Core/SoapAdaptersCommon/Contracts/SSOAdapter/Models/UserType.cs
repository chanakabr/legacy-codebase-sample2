using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public class UserType
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}