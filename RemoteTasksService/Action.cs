using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RemoteTasksService
{
    [Serializable]
    public class Action
    {
        [JsonProperty("action")]
        public string ActionImp { get; set; }
    }
}