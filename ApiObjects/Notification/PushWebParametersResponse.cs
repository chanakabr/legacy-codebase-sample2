using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class RegistryResponse
    {
        public Status Status { get; set; }
        public List<RegistryParameter> Items { get; set; }
    }
}
