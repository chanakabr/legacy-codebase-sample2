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
