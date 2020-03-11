using WebAPI.Models.General;
using System.Runtime.Serialization;
using Newtonsoft.Json;

// TODO: Arthur, move to a different namespace (Exceptions, need to update reflector as well because of this)
namespace WebAPI.App_Start
{
    [DataContract(Name = "error")]
    public partial class KalturaAPIExceptionWrapper : KalturaSerializable
    {
        [DataMember(Name = "error")]
        [JsonProperty(PropertyName = "error")]
        public KalturaAPIException error { get; set; }
    }
}
