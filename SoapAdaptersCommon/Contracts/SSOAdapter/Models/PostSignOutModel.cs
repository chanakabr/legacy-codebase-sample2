using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public partial class PostSignOutModel : SignOutModel
    {
        public User AuthenticatedUser;
    }
}
