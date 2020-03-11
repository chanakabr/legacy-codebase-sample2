using WebAPI.Models.General;
using System.Runtime.Serialization;
using Newtonsoft.Json;

// TODO: Arthur, move to a different namespace (Exceptions, need to update reflector as well because of this)
namespace WebAPI.App_Start
{
    public partial class KalturaApiExceptionArg : KalturaOTTObject
    {
        /// <summary>
        /// Argument name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        /// <summary>
        /// Argument value
        /// </summary>
        [DataMember(Name = "value")]
        [JsonProperty(PropertyName = "value")]
        public string value { get; set; }
    }
}
