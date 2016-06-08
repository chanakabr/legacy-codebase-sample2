using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class PushWebParametersResponse
    {
        public Status Status { get; set; }
        public List<PushWebParameter> Items { get; set; }
    }
}
