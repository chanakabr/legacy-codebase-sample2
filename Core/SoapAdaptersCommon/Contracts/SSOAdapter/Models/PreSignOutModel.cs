using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public partial class PreSignOutModel
    {
        [DataMember]
        public string DeviceUdid { get; set; }
        [DataMember]
        public int HouseholdId { get; set; }
        [DataMember]
        public int UserId { get; set; }
    }
}
