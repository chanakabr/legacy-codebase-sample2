using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace RemoteTasksService
{
    [DataContract]
    public class AddTaskRequest
    {
        [DataMember]
        public string task { get; set; }
        [DataMember]
        public string data { get; set; } 
    }
}