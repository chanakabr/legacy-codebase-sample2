using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using KLogMonitor;
using WebAPI.Models;
using WebAPI.Models.General;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using System.Runtime.Serialization;
using KlogMonitorHelper;
using Newtonsoft.Json;

// TODO: Arthur, move to a different namespace (Exceptions, need to update reflector as well because of this)
namespace WebAPI.App_Start
{
    public partial class KalturaAPIException : KalturaSerializable
    {
        [JsonProperty(PropertyName = "objectType")]
        [DataMember(Name = "objectType")]
        public string objectType { get { return this.GetType().Name; } set { } }

        [JsonProperty(PropertyName = "code")]
        [DataMember(Name = "code")]
        public string code { get; set; }

        [JsonProperty(PropertyName = "message")]
        [DataMember(Name = "message")]
        public string message { get; set; }

        [JsonProperty(PropertyName = "args")]
        [DataMember(Name = "args")]
        public List<KalturaApiExceptionArg> args { get; set; }
    }
}
